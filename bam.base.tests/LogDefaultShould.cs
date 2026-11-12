using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Test;

namespace Bam.Tests;

/// <summary>
/// Covers bam.base#48: replacing <see cref="Log.Default"/> stops the logger being replaced, not the new one, and
/// <see cref="Log.Info"/>, <see cref="Log.Warn"/> and <see cref="Log.Error(string, Exception, object[])"/> keep the
/// format spec the signature gives each argument.
/// </summary>
[UnitTestMenu("Log default should", Selector = "lgdf")]
public class LogDefaultShould : UnitTestMenuContainer
{
    public LogDefaultShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void StopTheLoggerItReplaces()
    {
        After.Setup(reg => reg.Set(new RecordingLogger()))
        .When<RecordingLogger>("sets Default twice and waits for the stop task", (first, reg) =>
        {
            ILogger? original = Log.Default;
            RecordingLogger second = new RecordingLogger();
            try
            {
                Log.Default = first;
                Log.Default = second;
                bool firstStopped = first.Stopped.Wait(TimeSpan.FromSeconds(5));
                Thread.Sleep(100);
                return new SwapOutcome(firstStopped, second.StopCount, ReferenceEquals(Log.Default, second));
            }
            finally
            {
                Log.Default = original;
            }
        })
        .TheTest
        .ShouldPass<SwapOutcome>((because, outcome) =>
        {
            because.ItsTrue("the replaced logger is stopped", outcome.ReplacedStopped);
            because.ItsTrue("the new logger is not stopped, stop count " + outcome.NewLoggerStops, outcome.NewLoggerStops == 0);
            because.ItsTrue("the new logger is the default", outcome.NewIsDefault);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void KeepFormatSpecs()
    {
        After.Setup(reg => reg.Set(new RecordingLogger()))
        .When<RecordingLogger>("logs a date and a number with format specs, plus a null", (logger, reg) =>
        {
            ILogger? original = Log.Default;
            DateTime when = new DateTime(2026, 11, 11, 8, 30, 0, DateTimeKind.Utc);
            try
            {
                Log.Default = logger;
                logger.StartLoggingThread();
                Log.Info("at {0:o} took {1,8:F2} {{literal:x}} by {2}", when, 1.5, null!);
                Log.Warn("{0:yyyy-MM-dd} then {0}", when);
                Log.Error("failed at {0:o}", new InvalidOperationException("x"), when);
                logger.BlockUntilEventQueueIsEmpty();
                List<string?[]> values = logger.Values();
                return new FormatOutcome(
                    string.Join("|", values[0].Select(v => v ?? "<null>")),
                    values[1][0],
                    values[2][0]);
            }
            finally
            {
                Log.Default = original;
            }
        })
        .TheTest
        .ShouldPass<FormatOutcome>((because, outcome) =>
        {
            because.ItsTrue("info keeps the round-trip date and the number spec, got " + outcome.Info,
                outcome.Info == "2026-11-11T08:30:00.0000000Z|" + 1.5.ToString("F2") + "|<null>");
            because.ItsTrue("warn uses the first spec of a repeated index, got " + outcome.Warn, outcome.Warn == "2026-11-11");
            because.ItsTrue("error keeps the spec, got " + outcome.Error, outcome.Error == "2026-11-11T08:30:00.0000000Z");
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void NeverThrowOnABadSpec()
    {
        After.Setup(reg => reg.Set(new RecordingLogger()))
        .When<RecordingLogger>("logs specs the types refuse, and one asking for a huge precision", (logger, reg) =>
        {
            ILogger? original = Log.Default;
            DateTime when = new DateTime(2026, 11, 11, 8, 30, 0, DateTimeKind.Utc);
            Guid id = new Guid("6d1f7c3a-2b4e-4f6a-9c8d-1e2f3a4b5c6d");
            string? thrown = null;
            try
            {
                Log.Default = logger;
                logger.StartLoggingThread();
                try
                {
                    Log.Info("{0:Q}", 5);
                    Log.Warn("{0:%}", when);
                    Log.Error("{0:z}", new InvalidOperationException("x"), id);
                    Log.Info("{0:D99999999}", 7);
                    Log.Info("{0:0000}", 7);
                }
                catch (Exception ex)
                {
                    thrown = ex.GetType().Name;
                }
                logger.BlockUntilEventQueueIsEmpty();
                List<string?[]> values = logger.Values();
                return new BadSpecOutcome(thrown, string.Join("|", values.Select(v => v[0])), when.ToString(), id.ToString());
            }
            finally
            {
                Log.Default = original;
            }
        })
        .TheTest
        .ShouldPass<BadSpecOutcome>((because, outcome) =>
        {
            because.ItsTrue("nothing throws, got " + (outcome.Thrown ?? "nothing"), outcome.Thrown is null);
            because.ItsTrue("each refused or oversized spec logs the plain value and a usable one still applies, got " + outcome.Values,
                outcome.Values == "5|" + outcome.PlainDate + "|" + outcome.PlainGuid + "|7|0007");
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    /// <summary>A logger that records what it commits and how often it is stopped.</summary>
    public sealed class RecordingLogger : Logger
    {
        private readonly List<string?[]> _values = new List<string?[]>();
        private readonly TaskCompletionSource<bool> _stopped = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _stopCount;

        /// <summary>Creates the logger.</summary>
        public RecordingLogger()
        {
            CommitCycleDelay = 0;
        }

        /// <summary>Completes the first time the logger is stopped.</summary>
        public Task Stopped => _stopped.Task;

        /// <summary>How many times the logger has been stopped.</summary>
        public int StopCount => Volatile.Read(ref _stopCount);

        /// <summary>The variable values of each committed event, in order.</summary>
        public List<string?[]> Values()
        {
            lock (_values)
            {
                return new List<string?[]>(_values);
            }
        }

        /// <inheritdoc />
        public override void CommitLogEvent(LogEvent logEvent)
        {
            lock (_values)
            {
                _values.Add(logEvent.MessageVariableValues);
            }
        }

        /// <inheritdoc />
        public override ILogger StopLoggingThread()
        {
            Interlocked.Increment(ref _stopCount);
            _stopped.TrySetResult(true);
            return base.StopLoggingThread();
        }
    }

    private sealed record SwapOutcome(bool ReplacedStopped, int NewLoggerStops, bool NewIsDefault);

    private sealed record FormatOutcome(string Info, string? Warn, string? Error);

    private sealed record BadSpecOutcome(string? Thrown, string Values, string PlainDate, string PlainGuid);
}
