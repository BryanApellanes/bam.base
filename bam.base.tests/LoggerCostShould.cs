using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Test;

namespace Bam.Tests;

/// <summary>Covers the per-event costs and the queue bound of <see cref="Logger"/> (bam.base#15) and message-safe formatting (bam.base#16).</summary>
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
                logger.WaitFor(entry => logger.Committed.Count >= 6);
                logger.StopLoggingThread();
                return outcome with { CommittedAfterRelease = logger.Committed.Count };
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
                // Measured at about 6.5 KB, mostly the diagnostic header; capturing a stack trace costs about 15 times that.
                // The bound catches a stack trace or a Process object creeping back onto the information path.
                because.ItsTrue($"an information event allocates under 12 KB ({outcome.Plain} bytes)", outcome.Plain < 12288);
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

    private static Exception Thrown()
    {
        try
        {
            throw new InvalidOperationException("boom", new Exception("inner"));
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

    private sealed record StackOutcome(string? Plain, string? Failure, string? Traced);

    private sealed record QueueOutcome(int Pending, long Dropped, int CommittedAfterRelease = 0);

    private sealed record VerbosityOutcome(bool ErrorCommitted, long Dropped, int InformationCommitted);

    private sealed record AllocationOutcome(long Plain, long WithTrace);

    private sealed record FormatOutcome(string Text, int ProcessId);
}
