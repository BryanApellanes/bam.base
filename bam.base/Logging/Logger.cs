/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Text;
using System.Diagnostics;
using Bam.Configuration;
using System.Collections.Concurrent;

namespace Bam.Logging
{
    /// <summary>
    /// Abstract base class for loggers that processes log events asynchronously via a background thread and a concurrent queue.
    /// </summary>
    public abstract class Logger : ILogger, IHasRequiredProperties
    {
        readonly ConcurrentQueue<LogEvent> _logEventQueue;
        Thread _loggingThread = null!;
        readonly AutoResetEvent _waitForEnqueueLogEvent;

        readonly List<string> requiredProperties;
        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class, setting up the event queue,
        /// default verbosity to Information, and starting the background logging thread.
        /// </summary>
        public Logger()
        {
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(OnDomainUnload);
            _logEventQueue = new ConcurrentQueue<LogEvent>();
            _waitForEnqueueLogEvent = new AutoResetEvent(false);

            requiredProperties = new List<string>
            {
                "LogType",
                "ApplicationName"
            };
            Verbosity = VerbosityLevel.Information;
            EventIdProvider = new HashingEventIdProvider();
            CommitCycleDelay = 30;
            StartLoggingThread();
        }

        protected virtual void OnDomainUnload(object? sender, EventArgs e)
        {
            BlockUntilEventQueueIsEmpty();
            StopLoggingThread();
        }

        /// <summary>
        /// Gets the names of configuration properties required by this logger ("LogType" and "ApplicationName").
        /// </summary>
        public string[] RequiredProperties
        {
            get { return requiredProperties.ToArray(); }
        }

        /// <summary>
        /// Stops and then restarts the background logging thread.
        /// </summary>
        /// <returns>This logger instance for method chaining.</returns>
        public virtual ILogger RestartLoggingThread()
        {
            StopLoggingThread();
            StartLoggingThread();
            return this;
        }

        readonly object _threadLock = new object();
        bool _keepLogging = true;
        /// <summary>
        /// Stops the background logging thread, waiting up to 3 seconds for it to finish before aborting.
        /// </summary>
        /// <returns>This logger instance for method chaining.</returns>
        public virtual ILogger StopLoggingThread()
        {
            if (_loggingThread != null)
            {
                lock (_threadLock)
                {
                    _keepLogging = false;
                    _loggingThreadStarted = false;
                    _waitForEnqueueLogEvent.Set();
                    int wait = 3000;
                    int waited = 0;
                    if (Exec.TakesTooLong(() =>
                    {
                        while (_loggingThread.ThreadState != System.Threading.ThreadState.AbortRequested &&
                           _loggingThread.ThreadState != System.Threading.ThreadState.Aborted &&
                           _loggingThread.ThreadState != System.Threading.ThreadState.Stopped)
                        {
                            Thread.Sleep(1);
                            waited++;
                            if (waited == wait)
                            {
                                break;
                            }
                        }
                    }, 3500))
                    {
                        try
                        {
                            _loggingThread.Abort();
                        }
                        catch { } // not all ThreadStates are valid for a call to Abort
                    }
                }
            }
            return this;
        }
        
        /// <summary>
        /// The number of milliseconds to wait after a LogEvent
        /// is queued before beginning the
        /// commit loop.
        /// </summary>
        public int CommitCycleDelay { get; set; }

        bool _loggingThreadStarted;
        /// <summary>
        /// Starts the background logging commit thread if it is not already running.
        /// </summary>
        /// <returns>This logger instance for method chaining.</returns>
        public virtual ILogger StartLoggingThread()
        {
            if (!_loggingThreadStarted)
            {
                lock (_threadLock)
                {
                    _loggingThreadStarted = true;
                    _loggingThread = new Thread(LoggingThread) { IsBackground = true };
                    _keepLogging = true;
                    _loggingThread.Start();
                }
            }
            return this;
        }

        /// <summary>
        /// The most events that may wait to be committed. When the queue is full, new events are dropped and
        /// counted in <see cref="DroppedEventCount"/> instead of growing memory without bound behind a slow sink.
        /// Zero or less means unbounded. Default 10,000.
        /// </summary>
        public int MaxQueueLength { get; set; } = 10000;

        /// <summary>
        /// How many Error and Fatal events may wait beyond <see cref="MaxQueueLength"/>, so a flood of lower-severity
        /// events at or above <see cref="Verbosity"/> cannot crowd out an error. Default 1,000; zero or less gives
        /// errors no extra room.
        /// </summary>
        public int ErrorQueueHeadroom { get; set; } = 1000;

        /// <summary>
        /// The message signature of the summary the commit thread writes once a backlog that dropped events has
        /// drained: the number of events dropped and the UTC time of the first drop. Read-only rather than const,
        /// so code that compares against it picks up a change in wording.
        /// </summary>
        public static readonly string DroppedEventsSummarySignature = "{0} log events were dropped because the log queue was full, starting at {1} UTC";

        long _droppedEventCount;
        long _droppedSinceSummary;
        long _firstDropTicks;

        /// <summary>The number of events dropped because the queue was full.</summary>
        public long DroppedEventCount => Interlocked.Read(ref _droppedEventCount);

        int _pendingEventCount;

        /// <summary>The number of events queued and not yet taken by the commit thread.</summary>
        public int PendingEventCount => Volatile.Read(ref _pendingEventCount);

        /// <summary>
        /// Whether an event without an exception records the calling thread's stack trace. Capturing a stack trace
        /// with file information on every event is expensive and leaks source paths into logs, so the default is
        /// false; events with an exception always carry the exception's stack trace.
        /// </summary>
        public bool CaptureStackTraceWithoutException { get; set; }

        protected virtual void QueueLogEvent(LogEvent logEvent)
        {
            if (!_loggingThreadStarted)
            {
                StartLoggingThread();
            }
            // Events below Verbosity would be discarded by the commit thread anyway; filtering them here keeps them
            // from taking queue slots, so a burst of chatty events can never push out an error. The commit-side
            // check stays so a runtime Verbosity change still applies to events already queued.
            if ((int)logEvent.Severity > (int)Verbosity)
            {
                return;
            }
            int limit = MaxQueueLength;
            if (limit > 0 && (int)logEvent.Severity <= (int)LogEventType.Error && logEvent.Severity != LogEventType.None && ErrorQueueHeadroom > 0)
            {
                limit = (int)Math.Min(int.MaxValue, (long)limit + ErrorQueueHeadroom);
            }
            if (limit > 0 && Interlocked.Increment(ref _pendingEventCount) > limit)
            {
                Interlocked.Decrement(ref _pendingEventCount);
                if (Interlocked.Increment(ref _droppedSinceSummary) == 1)
                {
                    Interlocked.CompareExchange(ref _firstDropTicks, DateTime.UtcNow.Ticks, 0);
                }
                if (Interlocked.Increment(ref _droppedEventCount) == 1)
                {
                    Trace.WriteLine($"{GetType().Name}: log queue full ({limit} pending); dropping events. See DroppedEventCount.");
                }
                return;
            }
            if (limit <= 0)
            {
                Interlocked.Increment(ref _pendingEventCount);
            }
            _logEventQueue.Enqueue(logEvent);
            _waitForEnqueueLogEvent.Set();
        }

        private void LoggingThread()
        {
            while (_keepLogging)
            {
                try
                {
                    _waitForEnqueueLogEvent.WaitOne();
                    Thread.Sleep(CommitCycleDelay);
                    while (_logEventQueue.Count > 0)
                    {
                        if (_logEventQueue.TryDequeue(out LogEvent? logEvent))
                        {
                            Interlocked.Decrement(ref _pendingEventCount);
                            if (logEvent != null && (int)logEvent.Severity <= (int)Verbosity)
                            {
                                CommitLogEvent(logEvent);
                            }
                        }
                    }
                    CommitDropSummary();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Exception in logging commit thread: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Once the backlog has drained, commits one summary saying how many events were dropped since the last
        /// summary and when the first of them was, so the loss is recorded in the log itself. The summary is a
        /// Warning, or carries <see cref="Verbosity"/>'s own severity when that is Error or Fatal, because in that
        /// configuration every dropped event was an error and a Warning would be filtered out. It is committed
        /// directly, so <see cref="EntryAdded"/> and the per-severity events do not see it.
        /// </summary>
        /// <remarks>
        /// The count and the first-drop time are two separate fields. A drop that lands while a summary is being
        /// taken can be counted in the next summary with this summary's start time, or with the commit time; the
        /// count is always exact, only the "starting at" time can be off by that one race.
        /// </remarks>
        private void CommitDropSummary()
        {
            long dropped = Interlocked.Exchange(ref _droppedSinceSummary, 0);
            if (dropped == 0)
            {
                return;
            }
            long firstTicks = Interlocked.Exchange(ref _firstDropTicks, 0);
            if (Verbosity == VerbosityLevel.None)
            {
                return;
            }
            LogEventType severity = (int)LogEventType.Warning > (int)Verbosity ? (LogEventType)(int)Verbosity : LogEventType.Warning;
            DateTime since = firstTicks == 0 ? DateTime.UtcNow : new DateTime(firstTicks, DateTimeKind.Utc);
            CommitLogEvent(CreateLogEvent(
                DroppedEventsSummarySignature,
                UserUtil.GetCurrentUser(true),
                "Application",
                severity,
                null!,
                dropped.ToString(System.Globalization.CultureInfo.InvariantCulture),
                since.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Blocks the current thread until the event queue empties.
        /// </summary>
        /// <param name="sleep">Additional milliseconds to sleep after the queue is empty.</param>
        public virtual void BlockUntilEventQueueIsEmpty(int sleep = 0)
        {
            try
            {
                if (_loggingThread != null && _loggingThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    _keepLogging = false;
                    while (_logEventQueue.Count > 0)
                    {
                        _waitForEnqueueLogEvent.Set();
                        Thread.Sleep(3);
                    }
                }
                Thread.Sleep(sleep);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception in {nameof(BlockUntilEventQueueIsEmpty)}: {ex.Message}");
            }
        }

        internal ConcurrentQueue<LogEvent> LogEventQueue
        {
            get
            {
                return _logEventQueue;
            }
        }

        #region ILogger Members

        /// <summary>
        /// Gets a value indicating whether this is a null (no-op) logger. Always returns false for concrete loggers.
        /// </summary>
        public virtual bool IsNull { get { return false; } }

        static readonly string CachedMachineName = Environment.MachineName;

        string appName = null!;
        long appNameRetryAt;
        /// <summary>
        /// Gets or sets the application name used in log entries. Resolved from configuration or <see cref="ApplicationNameProvider"/> if not explicitly set.
        /// While it resolves to the default name, it is looked up again at most every ten seconds, so configuration
        /// that loads late is still picked up without a configuration read on every access.
        /// </summary>
        public string ApplicationName
        {
            get
            {
                string current = appName;
                if (string.IsNullOrEmpty(current) || (current.Equals(DefaultConfiguration.DefaultApplicationName) && Environment.TickCount64 >= Volatile.Read(ref appNameRetryAt)))
                {
                    current = DefaultConfiguration.GetAppSetting("ApplicationName", ApplicationNameProvider.Default?.GetApplicationName() ?? DefaultConfiguration.DefaultApplicationName);
                    Volatile.Write(ref appNameRetryAt, Environment.TickCount64 + 10000);
                    appName = current;
                }
                return current;
            }
            set
            {
                appName = value;
                Volatile.Write(ref appNameRetryAt, 0);
            }
        }

        /// <summary>
        /// A number indicating what level of verbosity to log.  The default is 4.
        /// </summary>
        public VerbosityLevel Verbosity
        {
            get;
            set;
        }

        /// <summary>
        /// When overridden in a derived class, commits the specified log event
        /// to the underlying storage for the current Logger implementation.
        /// </summary>
        /// <param name="logEvent">The log event to persist or output.</param>
        public abstract void CommitLogEvent(LogEvent logEvent);

        /// <summary>
        /// Occurs when any log entry is added.
        /// </summary>
        public event LogEntryAddedListener? EntryAdded;

        /// <summary>
        /// Occurs when a fatal-level log event is added.
        /// </summary>
        public event LogEntryAddedListener? FatalEventOccurred;

        /// <summary>
        /// Occurs when an information-level log event is added.
        /// </summary>
        public event LogEntryAddedListener? InfoEventOccurred;

        /// <summary>
        /// Occurs when a warning-level log event is added.
        /// </summary>
        public event LogEntryAddedListener? WarnEventOccurred;

        /// <summary>
        /// Occurs when an error-level log event is added.
        /// </summary>
        public event LogEntryAddedListener? ErrorEventOccurred;

        /// <summary>
        /// Occurs when a custom-level log event is added.
        /// </summary>
#pragma warning disable CS0067
        public event LogEntryAddedListener? CustomEventOccurred;
#pragma warning restore CS0067

        /// <summary>
        /// Gets or sets the provider used to generate event IDs from application name and message signature. Defaults to <see cref="HashingEventIdProvider"/>.
        /// </summary>
        public virtual IEventIdProvider EventIdProvider { get; set; }

        /// <summary>
        /// Adds an information-level log entry with the specified message.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        public void AddEntry(string messageSignature)
        {
            AddEntry(messageSignature, new string[] { });
        }

        /// <summary>
        /// Adds a log entry with the specified message and verbosity level.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The verbosity level as an integer.</param>
        public virtual void AddEntry(string messageSignature, int verbosity)
        {
            AddEntry(messageSignature, verbosity, new string[] { });
        }

        /// <summary>
        /// Adds an error-level log entry with the specified message and exception.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        public virtual void AddEntry(string messageSignature, Exception ex)
        {
            AddEntry(messageSignature, ex, new string[] { });
        }

        /// <summary>
        /// Adds an information-level log entry with variable message values.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public virtual void AddEntry(string messageSignature, params string[] variableMessageValues)
        {
            AddEntry(messageSignature, UserUtil.GetCurrentUser(true), variableMessageValues);
        }

        /// <summary>
        /// Adds a log entry with the specified verbosity level and variable message values.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The verbosity level as an integer.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public virtual void AddEntry(string messageSignature, int verbosity, params string[] variableMessageValues)
        {
            AddEntry(messageSignature, UserUtil.GetCurrentUser(true), verbosity, variableMessageValues);
        }

        /// <summary>
        /// Adds an error-level log entry with the specified exception and variable message values.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public virtual void AddEntry(string messageSignature, Exception ex, params string?[] variableMessageValues)
        {
            AddEntry(messageSignature, UserUtil.GetCurrentUser(true), ex, variableMessageValues);
        }

        private void AddEntry(string messageSignature, string user, params string[] variableMessageValues)
        {
            AddEntry(messageSignature, user, (int)LogEventType.Information, variableMessageValues);
        }

        private void AddEntry(string messageSignature, string user, int verbosity, params string?[] variableMessageValues)
        {
            AddEntry(messageSignature, user, "Application", verbosity, variableMessageValues);
        }

        private void AddEntry(string messageSignature, string user, Exception ex, params string?[] variableMessageValues)
        {
            if (ex == null)
            {
                AddEntry(messageSignature, user, (int)LogEventType.Error, variableMessageValues);
            }
            else
            {
                AddEntry(messageSignature, user, "Application", ex, variableMessageValues!);
            }
        }

        private void AddEntry(string messageSignature, string user, string category, params string[] variableMessageValues)
        {
            AddEntry(messageSignature, user, category, (int)LogEventType.Information, variableMessageValues);
        }

        private void AddEntry(string messageSignature, string user, string category, int verbosity, params string?[] variableMessageValues)
        {
            // An Error logged without an exception carries no stack trace unless CaptureStackTraceWithoutException
            // asks for one: a synthetic exception here would have no trace of its own and force a costly capture.
            LogEvent ev = CreateLogEvent(messageSignature, user, category, (LogEventType)verbosity, null!, variableMessageValues);

            QueueLogEvent(ev);

            OnEntryAdded(ev);
        }

        private void OnEntryAdded(LogEvent logEvent)
        {
            EntryAdded?.Invoke(this.ApplicationName, logEvent);

            switch (logEvent.Severity)
            {
                case LogEventType.None:
                    break;
                case LogEventType.Information:
                    InfoEventOccurred?.Invoke(this.ApplicationName, logEvent);
                    break;
                case LogEventType.Warning:
                    WarnEventOccurred?.Invoke(this.ApplicationName, logEvent);
                    break;
                case LogEventType.Error:
                    ErrorEventOccurred?.Invoke(this.ApplicationName, logEvent);
                    break;
                case LogEventType.Fatal:
                    FatalEventOccurred?.Invoke(this.ApplicationName, logEvent);
                    break;
                default:
                    break;
            }
        }

        private void AddEntry(string messageSignature, string user, string category, Exception ex, params string[] variableMessageValues)
        {
            LogEvent ev = CreateLogEvent(messageSignature, user, category, LogEventType.Error, ex, variableMessageValues);

            QueueLogEvent(ev);

            OnEntryAdded(ev);
        }

        /// <summary>
        /// Adds a log entry with the specified verbosity level and exception.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The verbosity level as an integer.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        public void AddEntry(string messageSignature, int verbosity, Exception ex)
        {
            AddEntry(messageSignature, verbosity, ex, new string[] { });
        }

        /// <summary>
        /// Adds a log entry with the specified verbosity level, exception, and variable message values.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The verbosity level as an integer.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public void AddEntry(string messageSignature, int verbosity, Exception ex, params string[] variableMessageValues)
        {
            LogEvent ev = CreateLogEvent(messageSignature, UserUtil.GetCurrentUser(true), "Application", (LogEventType)verbosity, ex, variableMessageValues);

            QueueLogEvent(ev);

            OnEntryAdded(ev);
        }

        protected internal LogEvent CreateInfoEvent(string message)
        {
            return CreateInfoEvent(message, new string[] { });
        }

        protected internal LogEvent CreateInfoEvent(string messageSignature, params string[] messageVariableValues)
        {
            return CreateLogEvent(messageSignature, UserUtil.GetCurrentUser(true), "Application", LogEventType.Information, null!, messageVariableValues);
        }

        protected internal LogEvent CreateWarningEvent(string message)
        {
            return CreateWarningEvent(message, new string[] { });
        }

        protected internal LogEvent CreateWarningEvent(string messageSignature, params string[] messageVariableValues)
        {
            return CreateLogEvent(messageSignature, UserUtil.GetCurrentUser(true), "Application", LogEventType.Warning, null!, messageVariableValues);
        }

        protected internal LogEvent CreateErrorEvent(string message)
        {
            return CreateErrorEvent(message, new string[] { });
        }

        protected internal LogEvent CreateErrorEvent(string messageSignature, params string[] messageVariableValues)
        {
            return CreateLogEvent(messageSignature, UserUtil.GetCurrentUser(true), "Application", LogEventType.Error, null!, messageVariableValues);
        }

        protected internal LogEvent CreateErrorEvent(string messageSignature, Exception ex, params string[] messageVariableValues)
        {
            return CreateLogEvent(messageSignature, UserUtil.GetCurrentUser(true), "Application", LogEventType.Error, ex, messageVariableValues);
        }

        protected internal virtual LogEvent CreateLogEvent(string messageSignature, string user, string category, LogEventType type, Exception ex, params string?[] messageVariableValues)
        {
            string applicationName = this.ApplicationName;
            LogEvent ev = new LogEvent
            {
                MessageSignature = messageSignature,
                MessageVariableValues = messageVariableValues,
                EventID = GetEventId(applicationName, messageSignature),
                Time = DateTime.UtcNow,
                Category = category,
                Computer = CachedMachineName
            };
            if (messageVariableValues.Length > 0)
            {
                try
                {
                    ev.Message = string.Format(messageSignature, messageVariableValues);
                }
                catch //(Exception ex)
                {
                    ev.Message = messageSignature;
                }
            }
            else
            {
                ev.Message = messageSignature;
            }

            StringBuilder message = HandleDetails(ev);

            StringBuilder stack = new StringBuilder();
            HandleStackTrace(ex, message, stack);

            ev.Message = message.ToString();
            ev.StackTrace = stack.ToString();

            ev.Source = applicationName;
            ev.User = user;

            ev.Severity = type;

            return ev;
        }

        protected virtual StringBuilder HandleDetails(LogEvent ev)
        {
            ApplicationDiagnosticInfo details = new ApplicationDiagnosticInfo(ev) { ApplicationName = ApplicationName };
            return new StringBuilder(details.ToString());
        }

        protected virtual void HandleStackTrace(Exception ex, StringBuilder message, StringBuilder stack)
        {
            if (ex is null && !CaptureStackTraceWithoutException)
            {
                return;
            }
			Args.SetMessageAndStackTrace(ex!, message, stack);
        }

        /// <summary>
        /// Returns a unique event ID computed from the application name and message signature using the <see cref="EventIdProvider"/>.
        /// </summary>
        /// <param name="applicationName">The name of the application.</param>
        /// <param name="messageSignature">The message format string used to identify the event.</param>
        /// <returns>An integer event ID.</returns>
        protected virtual int GetEventId(string applicationName, string messageSignature)
        {
            return EventIdProvider.GetEventId(applicationName, messageSignature);
        }

        /// <summary>
        /// Adds a log entry with the specified message and event type.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The log event type indicating severity.</param>
        public void AddEntry(string messageSignature, LogEventType verbosity)
        {
            AddEntry(messageSignature, (int)verbosity);
        }

        /// <summary>
        /// Adds a log entry with the specified message, event type, and exception.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The log event type indicating severity.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        public void AddEntry(string messageSignature, LogEventType verbosity, Exception ex)
        {
            AddEntry(messageSignature, (int)verbosity, ex);
        }

        /// <summary>
        /// Adds a log entry with the specified message, event type, and variable message values.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="type">The log event type indicating severity.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public virtual void AddEntry(string messageSignature, LogEventType type, params string?[] variableMessageValues)
        {
            AddEntry(messageSignature, (int)type, variableMessageValues!);
        }

        /// <summary>
        /// Adds a log entry with the specified message, event type, exception, and variable message values.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="type">The log event type indicating severity.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public virtual void AddEntry(string messageSignature, LogEventType type, Exception ex, params string[] variableMessageValues)
        {
            AddEntry(messageSignature, (int)type, ex, variableMessageValues);
        }

        /// <summary>
        /// Adds a log entry with the specified message and verbosity level.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The verbosity level.</param>
        public virtual void AddEntry(string messageSignature, VerbosityLevel verbosity)
        {
            AddEntry(messageSignature, (int)verbosity);
        }

        /// <summary>
        /// Adds a log entry with the specified message, verbosity level, and exception.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The verbosity level.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        public virtual void AddEntry(string messageSignature, VerbosityLevel verbosity, Exception ex)
        {
            AddEntry(messageSignature, (int)verbosity, ex);
        }

        /// <summary>
        /// Adds a log entry with the specified message, verbosity level, and variable message values.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="type">The verbosity level.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public virtual void AddEntry(string messageSignature, VerbosityLevel type, params string[] variableMessageValues)
        {
            AddEntry(messageSignature, (int)type, variableMessageValues);
        }

        /// <summary>
        /// Adds a log entry with the specified message, verbosity level, exception, and variable message values.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="type">The verbosity level.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public virtual void AddEntry(string messageSignature, VerbosityLevel type, Exception ex, params string[] variableMessageValues)
        {
            AddEntry(messageSignature, (int)type, ex, variableMessageValues);
        }

        /// <summary>
        /// Adds an information-level log entry with the specified message and arguments.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message; converted to strings.</param>
        public void Info(string messageSignature, params object[] args)
        {
            Args.ThrowIfNull(args);
            AddEntry(messageSignature, LogEventType.Information, args.Each(a => a.ToString()).ToArray());
        }

        /// <summary>
        /// Adds a warning-level log entry with the specified message and arguments.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message; converted to strings.</param>
        public void Warning(string messageSignature, params object[] args)
        {
            Args.ThrowIfNull(args);
            AddEntry(messageSignature, LogEventType.Warning, args.Each(a => a.ToString()).ToArray());
        }

        /// <summary>
        /// Adds an error-level log entry with the specified message and arguments.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message; converted to strings.</param>
        public void Error(string messageSignature, params object[] args)
        {
            Args.ThrowIfNull(args);
            AddEntry(messageSignature, LogEventType.Error, args.Each(a => a.ToString()).ToArray());
        }

        /// <summary>
        /// Adds an error-level log entry with the specified message, exception, and arguments.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        /// <param name="args">Arguments to format into the message; converted to strings.</param>
        public void Error(string messageSignature, Exception? ex, params object[] args)
        {
            Args.ThrowIfNull(args);
            AddEntry(messageSignature, ex!, args.Each(a => a.ToString()).ToArray());
        }

        #endregion
    }
}
