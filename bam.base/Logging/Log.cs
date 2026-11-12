/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Globalization;
using System.Reflection;
using Bam.Configuration;
using Bam.Console;

namespace Bam.Logging
{
    /// <summary>
    /// Provides static convenience methods for application-wide logging, wrapping the default <see cref="ILogger"/> instance.
    /// </summary>
    public static partial class Log
    {
        static bool? _debug;
        /// <summary>
        /// Gets or sets whether debug output is enabled. Defaults to true if the --debug or -debug command-line argument is present.
        /// </summary>
        public static bool DebugOut
        {
            get
            {
                if (_debug == null)
                {
                    _debug = Environment.GetCommandLineArgs().Count(arg => arg.Equals("--debug") || arg.Equals("-debug") || arg.Equals("--debug")) > 0;
                }
                return _debug.Value;
            }
            set => _debug = value;
        }

        static bool? _trace;
        /// <summary>
        /// Gets or sets whether trace output is enabled. Defaults to true if the --trace command-line argument is present.
        /// </summary>
        public static bool TraceOut
        {
            get
            {
                if (_trace == null)
                {
                    _trace = Environment.GetCommandLineArgs().Count(arg => arg.Equals($"--trace")) > 0;
                }
                return _trace.Value;
            }
            set => _trace = value;
        }

        static ILogger? _defaultLogger;
        private static readonly object _defaultLoggerLock = new object();
        /// <summary>
        /// Gets or sets the default logger.  Default is determined by the configuration 
        /// file.
        /// </summary>
        public static ILogger? Default
        {
            get => _defaultLoggerLock.DoubleCheckLock(ref _defaultLogger, GetDefaultLogger);
            set
            {
                // Stop the logger being replaced, not whatever the field holds by the time the task runs.
                ILogger? outgoing = _defaultLogger;
                _defaultLogger = value;
                if (outgoing != null && !ReferenceEquals(outgoing, value))
                {
                    Task.Run(() => outgoing.StopLoggingThread());
                }
            }
        }

        /// <summary>
        /// Logs a warning entry only if the specified condition is true.
        /// </summary>
        /// <param name="condition">When true, the warning is logged.</param>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public static void WarnIf(bool condition, string messageSignature, params object[] args)
        {
            if (condition)
            {
                Warn(messageSignature, args);
            }
        }

        /// <summary>
        /// Logs an information-level entry to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public static void Info(string messageSignature, params object[] args)
        {
            Default!.AddEntry(messageSignature, LogEventType.Information, FormatArguments(messageSignature, args));
        }

        /// <summary>
        /// Logs a warning-level entry to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public static void Warn(string messageSignature, params object[] args)
        {
            Default!.AddEntry(messageSignature, LogEventType.Warning, FormatArguments(messageSignature, args));
        }

        /// <summary>
        /// Logs an error-level entry to the default logger, creating an exception from the formatted message.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public static void Error(string messageSignature, params object[] args)
        {
            Error(messageSignature, new Exception(string.Format(messageSignature, args)), args);
        }

        /// <summary>
        /// Logs an error-level entry to the default logger with an associated exception.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public static void Error(string messageSignature, Exception ex, params object[] args)
        {
            Default!.AddEntry(messageSignature, ex, FormatArguments(messageSignature, args));
        }

        /// <summary>
        /// Turns the arguments into the strings a log event carries, applying the format spec the signature
        /// gives each one (for example <c>{0:o}</c> or <c>{1:N2}</c>), so the logged text reads as
        /// <see cref="string.Format(string, object[])"/> would have rendered it.  A null argument stays null.
        /// When an index appears more than once, its first format spec is the one used.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">The arguments to render.</param>
        /// <returns>One string per argument.</returns>
        private static string[] FormatArguments(string messageSignature, object?[]? args)
        {
            if (args == null)
            {
                return null!;
            }
            Dictionary<int, string> specs = FormatSpecsOf(messageSignature ?? string.Empty);
            string[] values = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                object? arg = args[i];
                if (arg is IFormattable formattable && specs.TryGetValue(i, out string? spec))
                {
                    values[i] = formattable.ToString(spec, CultureInfo.CurrentCulture);
                }
                else
                {
                    values[i] = arg?.ToString()!;
                }
            }
            return values;
        }

        /// <summary>Reads the format spec of each format item in a composite format string, skipping escaped braces.</summary>
        private static Dictionary<int, string> FormatSpecsOf(string messageSignature)
        {
            Dictionary<int, string> specs = new Dictionary<int, string>();
            int position = 0;
            while (position < messageSignature.Length)
            {
                char current = messageSignature[position];
                if ((current == '{' || current == '}') && position + 1 < messageSignature.Length && messageSignature[position + 1] == current)
                {
                    position += 2;
                    continue;
                }
                if (current != '{')
                {
                    position++;
                    continue;
                }
                int close = messageSignature.IndexOf('}', position + 1);
                if (close < 0)
                {
                    break;
                }
                string item = messageSignature.Substring(position + 1, close - position - 1);
                int colon = item.IndexOf(':');
                string head = colon < 0 ? item : item.Substring(0, colon);
                int comma = head.IndexOf(',');
                string index = (comma < 0 ? head : head.Substring(0, comma)).Trim();
                if (colon >= 0 && int.TryParse(index, NumberStyles.None, CultureInfo.InvariantCulture, out int argument) && !specs.ContainsKey(argument))
                {
                    specs[argument] = item.Substring(colon + 1);
                }
                position = close + 1;
            }
            return specs;
        }

        /// <summary>
        /// Logs a debug message prefixed with the caller type name. Only outputs when in Dev mode and DebugOut is enabled.
        /// </summary>
        /// <param name="caller">The type of the calling class, used to prefix the message.</param>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public static void Debug(Type caller, string messageSignature, params object[] args)
        {
            Debug($"{caller.Name}::{messageSignature}", args);
        }

        /// <summary>
        /// Logs a debug message. Only outputs when in Dev mode and DebugOut is enabled.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public static void Debug(string messageSignature, params object[] args)
        {
            WriteDebug(messageSignature, args);
        }
        
        private static void WriteDebug(string messageSignature, object[] args)
        {
            if(ProcessMode.Current.Mode == ProcessModes.Dev)
            {
                if(DebugOut)
                {
                    Task.Run(() =>
                    {
                        Workspace.Current.WriteLine($"DEBUG: {messageSignature}", args);
                        string message = string.Format(messageSignature, args);
                        System.Diagnostics.Debug.WriteLine(message);
                    });
                }
            }
        }
        
        /// <summary>
        /// Logs a trace message prefixed with the caller type name. Only outputs when TraceOut is enabled.
        /// </summary>
        /// <param name="caller">The type of the calling class, used to prefix the message.</param>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public static void Trace(Type caller, string messageSignature, params object[] args)
        {
            Trace($"{caller?.GetType()?.Name}::{messageSignature}", args);
        }

        /// <summary>
        /// Logs a trace message. Only outputs when TraceOut is enabled.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public static void Trace(string messageSignature, params object[] args)
        {
            WriteTrace(messageSignature, args);
        }

        /// <summary>
        /// Logs a trace message with an associated exception. Only outputs when TraceOut is enabled.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="ex">The exception whose message and stack trace are appended to the output.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public static void Trace(string messageSignature, Exception ex, params object[] args)
        {
            WriteTrace($"{messageSignature}\r\n{ex.Message}\r\n{ex.StackTrace}", args);
        }

        private static void WriteTrace(string messageSignature, object[] args)
        {
            if (TraceOut)
            {
                Task.Run(() =>
                {
                    Workspace.Current.WriteLine($"TRACE: {messageSignature}", args);
                    string message = string.Format(messageSignature, args);
                    System.Diagnostics.Trace.WriteLine(message);
                });
            }
        }

        /// <summary>
        /// Resets the current logger to null, causing it to be re-initialized on next access. Used primarily for testing.
        /// </summary>
        public static void Reset()
        {
            lock (_currentLoggerLock)
            {
                _currentLogger = null!;
            }
        }

        static ILogger _currentLogger= null!;
        static object _currentLoggerLock = new object();
        private static ILogger GetDefaultLogger()
        {
            if (_currentLogger == null)
            {
                // create a logger of the type specified by the config
                // if no value is in the config create a null logger
                _currentLogger = CreateLogger(DefaultConfiguration.GetAppSetting("LogType", "Console"));
                _currentLogger.RestartLoggingThread();
            }

            return _currentLogger;
        }

        const string _loggingNamespace = "Bam.Logging";
        /// <summary>
        /// Creates a logger of the specified type.  If the containing assembly is already loaded 
        /// the type should be the namespace qualified name of the ILogger implementation to 
        /// instantiate.  If the containing assembly is not already loaded the type should be
        /// the AssemblyQualified name.  If the type is not found ConsoleLogger is returned.
        /// </summary>
        /// <param name="logType">The type name of the logger. Can be a fully qualified type name, a short name, or a name without the "Logger" suffix.</param>
        /// <returns>A new <see cref="ILogger"/> instance of the specified type, or a <see cref="ConsoleLogger"/> if the type cannot be resolved.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the resolved type does not have a parameterless constructor.</exception>
        public static ILogger CreateLogger(string logType)
        {
            lock (_currentLoggerLock)
            {
                string loggerTypeName = $"{logType}";
                Type loggerType = null!;
                try
                {
                    loggerType = Type.GetType(loggerTypeName)!;
                }
                catch
                {
                    loggerTypeName = $"{logType}Logger";
                    loggerType = Type.GetType(loggerTypeName)!;
                }
                
                if (loggerType == null)
                {
                    try
                    {
                        loggerType = Type.GetType($"{_loggingNamespace}.{logType}Logger")!;
                    }
                    catch
                    {
                        loggerType = null!;
                    }

                    if (loggerType == null)
                    {
                        try
                        {
                            loggerType = Type.GetType($"{_loggingNamespace}.{logType}")!;
                        }
                        catch
                        {
                            loggerType = null!;
                        }
                    }

                    if (loggerType == null)
                    {
                        return new ConsoleLogger();
                    }
                }

                return CreateLogger(loggerType);
            }
        }

        /// <summary>
        /// Creates a logger of the specified type. Requires a parameterless constructor; falls back to <see cref="ConsoleLogger"/> on failure.
        /// </summary>
        /// <param name="loggerType">The type of logger to instantiate.</param>
        /// <returns>A new <see cref="ILogger"/> instance of the specified type, or a <see cref="ConsoleLogger"/> if instantiation fails.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the specified type does not have a parameterless constructor.</exception>
        public static ILogger CreateLogger(Type loggerType)
        {
            try
            {
                ConstructorInfo ctor = loggerType.GetConstructor(Type.EmptyTypes)!;
                if (ctor == null)
                {
                    throw new InvalidOperationException($"The specified logType ({loggerType.FullName}) doesn't have a parameterless constructor.");
                }

                return ((ILogger)ctor.Invoke(null)).StartLoggingThread();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Failed to create logger of type ({0}): ({1})", loggerType.Name, ex.Message);
                return new ConsoleLogger();
            }
        }

        /// <summary>
        /// Creates a logger of the specified type and adds it to the current multi-target logger.
        /// If the current logger is not a multi-target logger, it is wrapped in one.
        /// </summary>
        /// <param name="loggerType">The type of logger to create and add.</param>
        /// <returns>The <see cref="IMultiTargetLogger"/> that now contains the added logger.</returns>
        public static IMultiTargetLogger AddLogger(Type loggerType)
        {
            return AddLogger(CreateLogger(loggerType));
        }

        /// <summary>
        /// Adds the specified logger instance to the current multi-target logger.
        /// If the current logger is not a multi-target logger, it is wrapped in one, preserving the existing logger.
        /// </summary>
        /// <param name="loggerInstance">The logger instance to add.</param>
        /// <returns>The <see cref="IMultiTargetLogger"/> that now contains the added logger.</returns>
        public static IMultiTargetLogger AddLogger(ILogger loggerInstance)
        {
            IMultiTargetLogger main = null!;

            if (_currentLogger == null)
            {
                _currentLogger = CreateLogger(typeof(IMultiTargetLogger));
                main = (IMultiTargetLogger)_currentLogger;
            }
            else
            {
                main = (_currentLogger as IMultiTargetLogger)!;
                if (main == null)
                {
                    main = (IMultiTargetLogger)CreateLogger(typeof(IMultiTargetLogger));
                    if (_currentLogger != null)
                    {
                        // add _currentLogger to the new MultiTargetLogger
                        // so it can continue to log
                        main.AddLogger(_currentLogger);
                    }

                    // whatever the _currentLogger was it wasn't a MultiTargetLogger
                    // set it here
                    lock (_currentLoggerLock)
                    {
                        if (_currentLogger is Logger current)
                        {
                            current.StopLoggingThread();
                        }
                        _currentLogger = main;
                    }
                }
            }

            main.AddLogger(loggerInstance);
            return main;
        }
        
        #region ILogger convenience methods.  Vanilla wrappers to the AddEntry and BlockUntilEventQueueIsEmpty methods of the Default ILogger

        /// <summary>
        /// Adds an information-level log entry with the specified message to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        public static void AddEntry(string messageSignature) { Default!.AddEntry(messageSignature); }

        /// <summary>
        /// Adds a log entry with the specified message and verbosity level to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The verbosity level as an integer.</param>
        public static void AddEntry(string messageSignature, int verbosity) { Default!.AddEntry(messageSignature, verbosity); }

        /// <summary>
        /// Adds a log entry with the specified message and event type to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="type">The type of log event.</param>
        public static void AddEntry(string messageSignature, LogEventType type) { Default!.AddEntry(messageSignature, type); }

        /// <summary>
        /// Adds an error-level log entry with the specified message and exception to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        public static void AddEntry(string messageSignature, Exception ex) { Default!.AddEntry(messageSignature, ex); }

        /// <summary>
        /// Adds a log entry with the specified message, verbosity level, and exception to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The verbosity level as an integer.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        public static void AddEntry(string messageSignature, int verbosity, Exception ex) { Default!.AddEntry(messageSignature, verbosity, ex); }

        /// <summary>
        /// Adds a log entry with the specified message, event type, and exception to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="type">The type of log event.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        public static void AddEntry(string messageSignature, LogEventType type, Exception ex) { Default!.AddEntry(messageSignature, type, ex); }

        /// <summary>
        /// Adds an information-level log entry with variable message values to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public static void AddEntry(string messageSignature, params string[] variableMessageValues) { Default!.AddEntry(messageSignature, variableMessageValues); }

        /// <summary>
        /// Adds a log entry with the specified verbosity and variable message values to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="verbosity">The verbosity level as an integer.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public static void AddEntry(string messageSignature, int verbosity, params string[] variableMessageValues) { Default!.AddEntry(messageSignature, verbosity, variableMessageValues); }

        /// <summary>
        /// Adds a log entry with the specified event type and variable message values to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="type">The type of log event.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public static void AddEntry(string messageSignature, LogEventType type, params string[] variableMessageValues) { Default!.AddEntry(messageSignature, type, variableMessageValues); }

        /// <summary>
        /// Adds a log entry with the specified verbosity, exception, and variable message values to the default logger.
        /// </summary>
        /// <param name="messagesignature">The message format string.</param>
        /// <param name="verbosity">The verbosity level as an integer.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public static void AddEntry(string messagesignature, int verbosity, Exception ex, params string[] variableMessageValues) { Default!.AddEntry(messagesignature, verbosity, ex, variableMessageValues); }

        /// <summary>
        /// Adds a log entry with the specified event type, exception, and variable message values to the default logger.
        /// </summary>
        /// <param name="messagesignature">The message format string.</param>
        /// <param name="type">The type of log event.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public static void AddEntry(string messagesignature, LogEventType type, Exception ex, params string[] variableMessageValues) { Default!.AddEntry(messagesignature, type, ex, variableMessageValues); }

        /// <summary>
        /// Adds an error-level log entry with the specified exception and variable message values to the default logger.
        /// </summary>
        /// <param name="messageSignature">The message format string.</param>
        /// <param name="ex">The exception to include in the log entry.</param>
        /// <param name="variableMessageValues">Values to substitute into the message format string.</param>
        public static void AddEntry(string messageSignature, Exception ex, params string[] variableMessageValues) { Default!.AddEntry(messageSignature, ex, variableMessageValues); }

        /// <summary>
        /// Blocks the current thread until the event queue is empty.  Keep
        /// in mind that other calls to AddEntry by other threads will
        /// increment the number of events in the queue.  If the commit
        /// thread is running it will be restarted.
        /// </summary>
        /// <param name="sleep">Additional milliseconds to sleep after the queue is empty.</param>
        public static void BlockUntilEventQueueIsEmpty(int sleep = 0) { Default!.BlockUntilEventQueueIsEmpty(sleep); }
        #endregion

        /// <summary>
        /// Restarts the background logging commit thread.
        /// </summary>
        public static void Restart()
        {
            Default!.RestartLoggingThread();
        }

        /// <summary>
        /// Starts the background logging commit thread.
        /// </summary>
        public static void Start()
        {
            Default!.StartLoggingThread();
        }

        /// <summary>
        /// Stops the background logging commit thread.
        /// </summary>
        public static void Stop()
        {
            Default!.StopLoggingThread();
        }        
    }
}
