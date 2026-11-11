using System.Diagnostics;
using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Test;

namespace Bam.Tests;

/// <summary>
/// Covers <see cref="Logger.BlockUntilEventQueueIsEmpty"/> (bam.base#42): it waits for every queued event to be
/// committed, leaves the logger running, and does not wait on a logger that has no commit thread.
/// </summary>
[UnitTestMenu("Logger drain should", Selector = "lgdr")]
public class LoggerDrainShould : UnitTestMenuContainer
{
    public LoggerDrainShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void CommitEveryQueuedEventBeforeReturning()
    {
        After.Setup(reg => reg.Set(new SlowLogger(20)))
        .When<SlowLogger>("queues a burst behind a slow sink and blocks until the queue is empty", (logger, reg) =>
        {
            for (int index = 0; index < 10; index++)
            {
                logger.AddEntry("burst {0}", LogEventType.Warning, index.ToString());
            }
            logger.BlockUntilEventQueueIsEmpty();
            int committedOnReturn = logger.CommittedCount;
            logger.StopLoggingThread();
            return new DrainOutcome(committedOnReturn);
        })
        .TheTest
        .ShouldPass<DrainOutcome>((because, outcome) =>
        {
            because.ItsTrue("all ten events are committed when the call returns, got " + outcome.CommittedOnReturn, outcome.CommittedOnReturn == 10);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void WaitForACommitThreadThatIsNotRunningYet()
    {
        After.Setup(reg => reg.Set(new SlowLogger(0) { CommitCycleDelay = 150 }))
        .When<SlowLogger>("blocks while the commit thread is sleeping out its commit cycle delay", (logger, reg) =>
        {
            logger.AddEntry("one", LogEventType.Warning);
            // The commit thread is in Thread.Sleep(CommitCycleDelay) here, so its ThreadState is WaitSleepJoin.
            logger.BlockUntilEventQueueIsEmpty();
            int committedOnReturn = logger.CommittedCount;
            logger.StopLoggingThread();
            return new DrainOutcome(committedOnReturn);
        })
        .TheTest
        .ShouldPass<DrainOutcome>((because, outcome) =>
        {
            because.ItsTrue("the event is committed when the call returns, got " + outcome.CommittedOnReturn, outcome.CommittedOnReturn == 1);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void LeaveTheLoggerRunning()
    {
        After.Setup(reg => reg.Set(new SlowLogger(1)))
        .When<SlowLogger>("logs, blocks, logs again and blocks again, five times over", (logger, reg) =>
        {
            List<int> counts = new List<int>();
            for (int round = 1; round <= 5; round++)
            {
                logger.AddEntry("round {0}", LogEventType.Information, round.ToString());
                logger.BlockUntilEventQueueIsEmpty();
                counts.Add(logger.CommittedCount);
            }
            logger.StopLoggingThread();
            return new RunningOutcome(string.Join(",", counts));
        })
        .TheTest
        .ShouldPass<RunningOutcome>((because, outcome) =>
        {
            because.ItsTrue("each round's event is committed after its own drain, got " + outcome.Counts, outcome.Counts == "1,2,3,4,5");
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ReturnPromptlyWithoutACommitThread()
    {
        After.Setup(reg => reg.Set(new SlowLogger(0)))
        .When<SlowLogger>("blocks on a logger that never logged, and on one whose thread was stopped", (logger, reg) =>
        {
            Stopwatch neverStarted = Stopwatch.StartNew();
            logger.BlockUntilEventQueueIsEmpty();
            neverStarted.Stop();

            logger.AddEntry("one", LogEventType.Warning);
            logger.BlockUntilEventQueueIsEmpty();
            logger.StopLoggingThread();
            Stopwatch stopped = Stopwatch.StartNew();
            logger.BlockUntilEventQueueIsEmpty();
            stopped.Stop();
            return new PromptOutcome(neverStarted.ElapsedMilliseconds, stopped.ElapsedMilliseconds, logger.CommittedCount);
        })
        .TheTest
        .ShouldPass<PromptOutcome>((because, outcome) =>
        {
            because.ItsTrue("a logger that never logged returns at once", outcome.NeverStartedMilliseconds < 1000);
            because.ItsTrue("a stopped logger returns at once", outcome.StoppedMilliseconds < 1000);
            because.ItsTrue("the event logged before the stop was committed", outcome.Committed == 1);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ReturnWhenTheSinkThrowsOnEveryEvent()
    {
        After.Setup(reg => reg.Set(new FailingLogger()))
        .When<FailingLogger>("queues a burst behind a sink that always throws and blocks until the queue is empty", (logger, reg) =>
        {
            for (int index = 0; index < 5; index++)
            {
                logger.AddEntry("burst {0}", LogEventType.Warning, index.ToString());
            }
            Task drain = Task.Run(() => logger.BlockUntilEventQueueIsEmpty());
            bool returned = drain.Wait(TimeSpan.FromSeconds(5));
            long pending = logger.PendingEventCount;
            int attempts = logger.Attempts;

            // The logger is still usable: a later event reaches the sink too.
            logger.AddEntry("after", LogEventType.Warning);
            Task second = Task.Run(() => logger.BlockUntilEventQueueIsEmpty());
            bool returnedAgain = second.Wait(TimeSpan.FromSeconds(5));
            int attemptsAfter = logger.Attempts;
            logger.StopLoggingThread();
            return new FailingSinkOutcome(returned, pending, attempts, returnedAgain, attemptsAfter);
        })
        .TheTest
        .ShouldPass<FailingSinkOutcome>((because, outcome) =>
        {
            because.ItsTrue("the call returns although every commit threw", outcome.Returned);
            because.ItsTrue("nothing is left queued, pending " + outcome.Pending, outcome.Pending == 0);
            because.ItsTrue("every event was offered to the sink, attempts " + outcome.Attempts, outcome.Attempts == 5);
            because.ItsTrue("a second drain returns too", outcome.ReturnedAgain);
            because.ItsTrue("the later event was offered to the sink, attempts " + outcome.AttemptsAfter, outcome.AttemptsAfter == 6);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ReturnWhenCalledFromTheCommitThread()
    {
        After.Setup(reg => reg.Set(new ReentrantLogger()))
        .When<ReentrantLogger>("has the sink flush its own logger while committing", (logger, reg) =>
        {
            logger.AddEntry("one", LogEventType.Warning);
            Task drain = Task.Run(() => logger.BlockUntilEventQueueIsEmpty());
            bool returned = drain.Wait(TimeSpan.FromSeconds(5));
            int committed = logger.CommittedCount;
            logger.StopLoggingThread();
            return new ReentrantOutcome(returned, committed);
        })
        .TheTest
        .ShouldPass<ReentrantOutcome>((because, outcome) =>
        {
            because.ItsTrue("the sink's own flush does not deadlock the commit thread", outcome.Returned);
            because.ItsTrue("the event is committed", outcome.Committed == 1);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    /// <summary>A logger whose sink throws on every event and counts the attempts.</summary>
    public sealed class FailingLogger : Logger
    {
        private int _attempts;

        /// <summary>Creates the logger with a commit delay long enough for a burst to queue up behind one signal.</summary>
        public FailingLogger()
        {
            CommitCycleDelay = 100;
        }

        /// <summary>How many events were offered to the sink.</summary>
        public int Attempts => Volatile.Read(ref _attempts);

        /// <inheritdoc />
        public override void CommitLogEvent(LogEvent logEvent)
        {
            Interlocked.Increment(ref _attempts);
            throw new IOException("the sink is unavailable");
        }
    }

    /// <summary>A logger whose sink calls <see cref="Logger.BlockUntilEventQueueIsEmpty"/> on itself.</summary>
    public sealed class ReentrantLogger : Logger
    {
        private int _committed;

        /// <summary>Creates the logger with no commit delay.</summary>
        public ReentrantLogger()
        {
            CommitCycleDelay = 0;
        }

        /// <summary>How many events the sink has finished committing.</summary>
        public int CommittedCount => Volatile.Read(ref _committed);

        /// <inheritdoc />
        public override void CommitLogEvent(LogEvent logEvent)
        {
            BlockUntilEventQueueIsEmpty();
            Interlocked.Increment(ref _committed);
        }
    }

    /// <summary>A logger whose sink takes a fixed time per event and counts what it committed.</summary>
    public sealed class SlowLogger : Logger
    {
        private readonly int _commitMilliseconds;
        private int _committed;

        /// <summary>Creates the logger.</summary>
        /// <param name="commitMilliseconds">How long each commit takes.</param>
        public SlowLogger(int commitMilliseconds)
        {
            _commitMilliseconds = commitMilliseconds;
            CommitCycleDelay = 0;
        }

        /// <summary>How many events the sink has finished committing.</summary>
        public int CommittedCount => Volatile.Read(ref _committed);

        /// <inheritdoc />
        public override void CommitLogEvent(LogEvent logEvent)
        {
            Thread.Sleep(_commitMilliseconds);
            Interlocked.Increment(ref _committed);
        }
    }

    private sealed record DrainOutcome(int CommittedOnReturn);

    private sealed record FailingSinkOutcome(bool Returned, long Pending, int Attempts, bool ReturnedAgain, int AttemptsAfter);

    private sealed record ReentrantOutcome(bool Returned, int Committed);

    private sealed record RunningOutcome(string Counts);

    private sealed record PromptOutcome(long NeverStartedMilliseconds, long StoppedMilliseconds, int Committed);
}
