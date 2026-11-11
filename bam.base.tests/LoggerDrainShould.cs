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

    private sealed record RunningOutcome(string Counts);

    private sealed record PromptOutcome(long NeverStartedMilliseconds, long StoppedMilliseconds, int Committed);
}
