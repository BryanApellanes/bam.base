using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Test;

namespace Bam.Tests;

/// <summary>
/// Covers the per-event costs and the queue bound of <see cref="Logger"/> (bam.base#15), message-safe formatting
/// (bam.base#16), error headroom and the drop summary (bam.base#18), stack traces for errors without an exception
/// (bam.base#19), the cheaper diagnostic header (bam.base#20) and escaped exception messages (bam.base#23).
/// </summary>
[UnitTestMenu("Logger cost should")]
public class LoggerCostShould : UnitTestMenuContainer
{
    public LoggerCostShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void CaptureAStackTraceOnlyForExceptionsUnlessAskedTo()
    {
        When.A<RecordingLogger>("logs with and without exceptions",
            () => new RecordingLogger(),
            logger =>
            {
                logger.AddEntry("plain information", LogEventType.Information);
                logger.AddEntry("failure", LogEventType.Error, Thrown());
                RecordingLogger opted = new RecordingLogger { CaptureStackTraceWithoutException = true };
                opted.AddEntry("traced information", LogEventType.Information);
                LogEvent? plain = logger.WaitFor(entry => entry.MessageSignature == "plain information");
                LogEvent? failure = logger.WaitFor(entry => entry.MessageSignature == "failure");
                LogEvent? traced = opted.WaitFor(entry => entry.MessageSignature == "traced information");
                logger.StopLoggingThread();
                opted.StopLoggingThread();
                return new StackOutcome(plain?.StackTrace, failure?.StackTrace, traced?.StackTrace);
            })
            .TheTest
            .ShouldPass<StackOutcome>((because, outcome) =>
            {
                because.ItsTrue("an information event carries no stack trace", string.IsNullOrEmpty(outcome.Plain));
                because.ItsTrue("an event with an exception still carries one", !string.IsNullOrWhiteSpace(outcome.Failure));
                because.ItsTrue("opting in captures the calling stack", !string.IsNullOrWhiteSpace(outcome.Traced));
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void BoundTheQueueAndCountDrops()
    {
        When.A<RecordingLogger>("logs behind a blocked sink",
            () => new RecordingLogger { MaxQueueLength = 5 },
            logger =>
            {
                logger.Block();
                logger.AddEntry("first", LogEventType.Information);
                SpinWait.SpinUntil(() => logger.PendingEventCount == 0 && logger.Committing, TimeSpan.FromSeconds(5));
                for (int index = 0; index < 20; index++)
                {
                    logger.AddEntry("burst {0}", LogEventType.Information, index.ToString());
                }
                QueueOutcome outcome = new QueueOutcome(logger.PendingEventCount, logger.DroppedEventCount);
                logger.Release();
                logger.WaitFor(entry => entry.MessageSignature == Logger.DroppedEventsSummarySignature);
                logger.StopLoggingThread();
                return outcome with { CommittedAfterRelease = logger.Committed.Count(entry => entry.MessageSignature != Logger.DroppedEventsSummarySignature) };
            })
            .TheTest
            .ShouldPass<QueueOutcome>((because, outcome) =>
            {
                because.ItsTrue("pending stays at the bound", outcome.Pending == 5);
                because.ItsTrue("the overflow is dropped and counted", outcome.Dropped == 15);
                because.ItsTrue("the queued events still commit once the sink frees up", outcome.CommittedAfterRelease == 6);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void NeverLetEventsBelowVerbosityCrowdOutErrors()
    {
        When.A<RecordingLogger>("queues only events that will be committed",
            () => new RecordingLogger { MaxQueueLength = 5, Verbosity = VerbosityLevel.Warning },
            logger =>
            {
                logger.Block();
                logger.AddEntry("first warning", LogEventType.Warning);
                SpinWait.SpinUntil(() => logger.PendingEventCount == 0 && logger.Committing, TimeSpan.FromSeconds(5));
                for (int index = 0; index < 20; index++)
                {
                    logger.AddEntry("chatty {0}", LogEventType.Information, index.ToString());
                }
                logger.AddEntry("the error", LogEventType.Error);
                long dropped = logger.DroppedEventCount;
                logger.Release();
                LogEvent? error = logger.WaitFor(entry => entry.MessageSignature == "the error");
                logger.StopLoggingThread();
                return new VerbosityOutcome(error is not null, dropped, logger.Committed.Count(entry => entry.Severity == LogEventType.Information));
            })
            .TheTest
            .ShouldPass<VerbosityOutcome>((because, outcome) =>
            {
                because.ItsTrue("the error commits behind a burst of information events", outcome.ErrorCommitted);
                because.ItsTrue("filtered events are not counted as drops", outcome.Dropped == 0);
                because.ItsTrue("no information event was queued", outcome.InformationCommitted == 0);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void KeepRoomForErrorsWhenWarningsFillTheQueue()
    {
        When.A<RecordingLogger>("logs an error behind a full queue of warnings",
            () => new RecordingLogger { MaxQueueLength = 5, ErrorQueueHeadroom = 2 },
            logger =>
            {
                logger.Block();
                logger.AddEntry("first warning", LogEventType.Warning);
                SpinWait.SpinUntil(() => logger.PendingEventCount == 0 && logger.Committing, TimeSpan.FromSeconds(5));
                for (int index = 0; index < 10; index++)
                {
                    logger.AddEntry("warning {0}", LogEventType.Warning, index.ToString());
                }
                logger.AddEntry("the error", LogEventType.Error);
                logger.AddEntry("the fatal", LogEventType.Fatal);
                logger.AddEntry("one error too many", LogEventType.Error);
                long dropped = logger.DroppedEventCount;
                logger.Release();
                LogEvent? error = logger.WaitFor(entry => entry.MessageSignature == "the error");
                LogEvent? fatal = logger.WaitFor(entry => entry.MessageSignature == "the fatal");
                logger.StopLoggingThread();
                return new HeadroomOutcome(error is not null, fatal is not null, dropped);
            })
            .TheTest
            .ShouldPass<HeadroomOutcome>((because, outcome) =>
            {
                because.ItsTrue("an error still commits behind a full queue of warnings", outcome.ErrorCommitted);
                because.ItsTrue("a fatal does too", outcome.FatalCommitted);
                because.ItsTrue("five warnings and the error past the headroom are dropped", outcome.Dropped == 6);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void CommitOneSummaryOfTheDropsOnceTheBacklogDrains()
    {
        When.A<RecordingLogger>("drops a burst and then drains",
            () => new RecordingLogger { MaxQueueLength = 3 },
            logger =>
            {
                logger.Block();
                logger.AddEntry("first", LogEventType.Information);
                SpinWait.SpinUntil(() => logger.PendingEventCount == 0 && logger.Committing, TimeSpan.FromSeconds(5));
                for (int index = 0; index < 10; index++)
                {
                    logger.AddEntry("burst {0}", LogEventType.Information, index.ToString());
                }
                logger.Release();
                LogEvent? summary = logger.WaitFor(entry => entry.MessageSignature == Logger.DroppedEventsSummarySignature);
                logger.AddEntry("after the burst", LogEventType.Information);
                logger.WaitFor(entry => entry.MessageSignature == "after the burst");
                logger.StopLoggingThread();
                List<LogEvent> summaries = logger.Committed.Where(entry => entry.MessageSignature == Logger.DroppedEventsSummarySignature).ToList();
                return new SummaryOutcome(summaries.Count, summary?.Severity, summary?.MessageVariableValues?.FirstOrDefault(), logger.DroppedEventCount);
            })
            .TheTest
            .ShouldPass<SummaryOutcome>((because, outcome) =>
            {
                because.ItsTrue("exactly one summary is committed", outcome.Summaries == 1);
                because.ItsTrue("it is a warning", outcome.Severity == LogEventType.Warning);
                because.ItsTrue("it carries the number dropped", outcome.Count == "7" && outcome.Dropped == 7);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void RecordDroppedErrorsWhenOnlyErrorsAreLogged()
    {
        When.A<RecordingLogger>("drops a burst of errors with Verbosity at Error",
            () => new RecordingLogger { MaxQueueLength = 3, ErrorQueueHeadroom = 0, Verbosity = VerbosityLevel.Error },
            logger =>
            {
                logger.Block();
                logger.AddEntry("first error", LogEventType.Error);
                SpinWait.SpinUntil(() => logger.PendingEventCount == 0 && logger.Committing, TimeSpan.FromSeconds(5));
                for (int index = 0; index < 8; index++)
                {
                    logger.AddEntry("error {0}", LogEventType.Error, index.ToString());
                }
                logger.Release();
                LogEvent? summary = logger.WaitFor(entry => entry.MessageSignature == Logger.DroppedEventsSummarySignature);
                logger.StopLoggingThread();
                int summaries = logger.Committed.Count(entry => entry.MessageSignature == Logger.DroppedEventsSummarySignature);
                return new ErrorsOnlyOutcome(summaries, summary?.Severity, summary?.MessageVariableValues?.FirstOrDefault());
            })
            .TheTest
            .ShouldPass<ErrorsOnlyOutcome>((because, outcome) =>
            {
                because.ItsTrue("the loss is still recorded", outcome.Summaries == 1);
                because.ItsTrue("at the logger's own severity, so Verbosity does not filter it", outcome.Severity == LogEventType.Error);
                because.ItsTrue("with the number of errors dropped", outcome.Count == "5");
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void RecordNoStackTraceForAnErrorLoggedWithoutAnException()
    {
        When.A<RecordingLogger>("logs errors with and without exceptions",
            () => new RecordingLogger(),
            logger =>
            {
                logger.AddEntry("bare error", LogEventType.Error);
                logger.AddEntry("bare error {0}", (int)LogEventType.Error, "with a value");
                logger.AddEntry("thrown error", LogEventType.Error, Thrown());
                RecordingLogger opted = new RecordingLogger { CaptureStackTraceWithoutException = true };
                opted.AddEntry("traced error", LogEventType.Error);
                LogEvent? bare = logger.WaitFor(entry => entry.MessageSignature == "bare error");
                LogEvent? bareWithValue = logger.WaitFor(entry => entry.MessageSignature == "bare error {0}");
                LogEvent? thrown = logger.WaitFor(entry => entry.MessageSignature == "thrown error");
                LogEvent? traced = opted.WaitFor(entry => entry.MessageSignature == "traced error");
                logger.StopLoggingThread();
                opted.StopLoggingThread();
                return new ErrorTraceOutcome(bare?.StackTrace, bareWithValue?.StackTrace, bareWithValue?.Severity, thrown?.StackTrace, traced?.StackTrace, bare?.Message);
            })
            .TheTest
            .ShouldPass<ErrorTraceOutcome>((because, outcome) =>
            {
                because.ItsTrue("an error without an exception carries no stack trace", string.IsNullOrEmpty(outcome.Bare));
                because.ItsTrue("nor through the verbosity overload", string.IsNullOrEmpty(outcome.BareWithValue) && outcome.BareWithValueSeverity == LogEventType.Error);
                because.ItsTrue("an error with a thrown exception carries that exception's trace", outcome.Thrown?.Contains(nameof(Thrown), StringComparison.Ordinal) == true);
                because.ItsTrue("opting in still captures the calling stack", !string.IsNullOrWhiteSpace(outcome.Traced));
                because.ItsTrue("no synthetic exception text is added", outcome.BareMessage?.Contains("A custom error event has been logged", StringComparison.Ordinal) == false);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void EscapeControlCharactersInExceptionMessages()
    {
        When.A<RecordingLogger>("logs exceptions whose messages carry CR, LF and other control characters",
            () => new RecordingLogger(),
            logger =>
            {
                logger.AddEntry("forged", LogEventType.Error, Thrown("a\r\nb\u0007c", "inner\nline"));
                logger.AddEntry("plain", LogEventType.Error, Thrown("ordinary message", "inner"));
                LogEvent? forged = logger.WaitFor(entry => entry.MessageSignature == "forged");
                LogEvent? plain = logger.WaitFor(entry => entry.MessageSignature == "plain");
                logger.StopLoggingThread();
                return new EscapeOutcome(forged?.Message ?? string.Empty, plain?.Message ?? string.Empty);
            })
            .TheTest
            .ShouldPass<EscapeOutcome>((because, outcome) =>
            {
                because.ItsTrue("CR and LF are written as visible escapes", outcome.Forged.Contains("a\\r\\nb\\u0007c", StringComparison.Ordinal));
                because.ItsTrue("the raw sequence does not appear", !outcome.Forged.Contains("a\r\nb", StringComparison.Ordinal));
                because.ItsTrue("the inner message is escaped too", outcome.Forged.Contains("inner\\nline", StringComparison.Ordinal));
                because.ItsTrue("a message without control characters is unchanged", outcome.Plain.Contains("\r\nordinary message\r\n", StringComparison.Ordinal));
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void WriteTheDefaultHeaderAsTheGeneralFormatterWould()
    {
        When.A<ApplicationDiagnosticInfo>("formats the default header on the fast path",
            () => new ApplicationDiagnosticInfo { Message = "hello {Message}", ApplicationName = "app" },
            info =>
            {
                FormatProbe probe = new FormatProbe
                {
                    Message = info.Message,
                    ApplicationName = info.ApplicationName,
                    Utc = info.Utc,
                    ThreadHashCode = info.ThreadHashCode,
                    ThreadId = info.ThreadId,
                    ProcessId = info.ProcessId,
                };
                return new HeaderOutcome(info.ToString(), probe.ToString());
            })
            .TheTest
            .ShouldPass<HeaderOutcome>((because, outcome) =>
            {
                because.ItsTrue("the fast path writes exactly what the reflective path writes", outcome.Fast == outcome.General);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void AllocateLittleForAnInformationEvent()
    {
        When.A<RecordingLogger>("measures what queuing an information event allocates",
            () => new RecordingLogger(),
            logger =>
            {
                RecordingLogger traced = new RecordingLogger { CaptureStackTraceWithoutException = true };
                logger.Block();
                traced.Block();
                for (int index = 0; index < 50; index++)
                {
                    logger.AddEntry("warm {0}", LogEventType.Information, index.ToString());
                    traced.AddEntry("warm {0}", LogEventType.Information, index.ToString());
                }
                long plain = Allocated(() => logger.AddEntry("measured {0}", LogEventType.Information, "value"));
                long withTrace = Allocated(() => traced.AddEntry("measured {0}", LogEventType.Information, "value"));
                logger.Release();
                traced.Release();
                logger.StopLoggingThread();
                traced.StopLoggingThread();
                return new AllocationOutcome(plain, withTrace);
            })
            .TheTest
            .ShouldPass<AllocationOutcome>((because, outcome) =>
            {
                // Measured at about 6.7 KB before bam.base#20 and about 1.7 KB after; capturing a stack trace costs many times that.
                // The bound catches reflection, a configuration read, a stack trace or a Process object creeping back
                // onto the information path.
                because.ItsTrue($"an information event allocates under 3 KB ({outcome.Plain} bytes)", outcome.Plain < 3072);
                because.ItsTrue($"capturing a stack trace costs clearly more ({outcome.WithTrace} bytes)", outcome.WithTrace > outcome.Plain * 2);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void WriteTokenLikeMessageTextAsItIs()
    {
        When.A<ApplicationDiagnosticInfo>("formats a message containing named tokens",
            () => new ApplicationDiagnosticInfo { Message = "GET /{ProcessId}/{ThreadId} {Message} {ApplicationName}", ApplicationName = "app" },
            info => new FormatOutcome(info.ToString(), info.ProcessId))
            .TheTest
            .ShouldPass<FormatOutcome>((because, outcome) =>
            {
                because.ItsTrue("the message text is written unchanged", outcome.Text.EndsWith("GET /{ProcessId}/{ThreadId} {Message} {ApplicationName}", StringComparison.Ordinal));
                because.ItsTrue("the header tokens are still filled in", outcome.Text.Contains($"PID={Environment.ProcessId}", StringComparison.Ordinal) && outcome.Text.Contains("App=app", StringComparison.Ordinal));
                because.ItsTrue("the process id is the current process", outcome.ProcessId == Environment.ProcessId);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    private static long Allocated(Action action)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    private static Exception Thrown(string message = "boom", string inner = "inner")
    {
        try
        {
            throw new InvalidOperationException(message, new Exception(inner));
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }
    }

    /// <summary>A logger that records committed events and can hold its commit thread.</summary>
    public sealed class RecordingLogger : Logger
    {
        private readonly ManualResetEventSlim _gate = new ManualResetEventSlim(true);
        private readonly List<LogEvent> _committed = new List<LogEvent>();
        private volatile bool _committing;

        /// <summary>Creates the logger with no commit delay.</summary>
        public RecordingLogger()
        {
            CommitCycleDelay = 0;
        }

        /// <summary>Whether the commit thread is inside <see cref="CommitLogEvent"/>.</summary>
        public bool Committing => _committing;

        /// <summary>The events committed so far.</summary>
        public IReadOnlyList<LogEvent> Committed
        {
            get
            {
                lock (_committed)
                {
                    return _committed.ToList();
                }
            }
        }

        /// <summary>Holds the commit thread at the next event.</summary>
        public void Block() => _gate.Reset();

        /// <summary>Lets the commit thread continue.</summary>
        public void Release() => _gate.Set();

        /// <summary>Waits up to five seconds for a committed event that matches.</summary>
        /// <param name="predicate">The match.</param>
        public LogEvent? WaitFor(Func<LogEvent, bool> predicate)
        {
            LogEvent? found = null;
            SpinWait.SpinUntil(() => (found = Committed.FirstOrDefault(predicate)) is not null, TimeSpan.FromSeconds(5));
            return found;
        }

        /// <inheritdoc />
        public override void CommitLogEvent(LogEvent logEvent)
        {
            _committing = true;
            _gate.Wait();
            lock (_committed)
            {
                _committed.Add(logEvent);
            }
            _committing = false;
        }
    }

    /// <summary>A subclass, so <see cref="ApplicationDiagnosticInfo.ToString"/> takes the general, reflective path.</summary>
    public sealed class FormatProbe : ApplicationDiagnosticInfo
    {
    }

    private sealed record ErrorsOnlyOutcome(int Summaries, LogEventType? Severity, string? Count);

    private sealed record HeadroomOutcome(bool ErrorCommitted, bool FatalCommitted, long Dropped);

    private sealed record SummaryOutcome(int Summaries, LogEventType? Severity, string? Count, long Dropped);

    private sealed record ErrorTraceOutcome(string? Bare, string? BareWithValue, LogEventType? BareWithValueSeverity, string? Thrown, string? Traced, string? BareMessage);

    private sealed record EscapeOutcome(string Forged, string Plain);

    private sealed record HeaderOutcome(string Fast, string General);

    private sealed record StackOutcome(string? Plain, string? Failure, string? Traced);

    private sealed record QueueOutcome(int Pending, long Dropped, int CommittedAfterRelease = 0);

    private sealed record VerbosityOutcome(bool ErrorCommitted, long Dropped, int InformationCommitted);

    private sealed record AllocationOutcome(long Plain, long WithTrace);

    private sealed record FormatOutcome(string Text, int ProcessId);
}
