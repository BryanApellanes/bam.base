using System.Runtime.CompilerServices;
using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Test;

namespace Bam.Tests;

/// <summary>
/// Covers how <see cref="Loggable"/> tracks its instances (bam.base#35): concurrent construction, derived types whose
/// equality is not usable during construction, and collection of instances nothing else references.
/// </summary>
[UnitTestMenu("Loggable instances should")]
public class LoggableInstancesShould : UnitTestMenuContainer
{
    public LoggableInstancesShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void TrackEveryInstanceConstructedConcurrently()
    {
        When.A<int>("constructs instances on many threads at once, each equal to the others and unhashable until built",
            () => 64 * 200,
            count =>
            {
                ConstructionSensitive[] built = new ConstructionSensitive[count];
                Exception? failure = null;
                try
                {
                    Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = 64 }, index => built[index] = new ConstructionSensitive());
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
                HashSet<ILoggable> tracked = new HashSet<ILoggable>(Loggable.AllInstances, ReferenceEqualityComparer.Instance);
                int missing = built.Count(instance => instance is null || !tracked.Contains(instance));
                GC.KeepAlive(built);
                return new ConcurrencyOutcome(failure?.ToString(), missing);
            })
            .TheTest
            .ShouldPass<ConcurrencyOutcome>((because, outcome) =>
            {
                because.ItsTrue($"no construction throws ({outcome.Failure})", outcome.Failure is null);
                because.ItsTrue($"every instance is tracked ({outcome.Missing} missing)", outcome.Missing == 0);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void LetAnUnreferencedInstanceBeCollected()
    {
        When.A<int>("tracks an instance, drops every reference to it and collects",
            () => 0,
            _ =>
            {
                WeakReference reference = Construct();
                for (int attempt = 0; attempt < 10 && reference.IsAlive; attempt++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
                return reference.IsAlive;
            })
            .TheTest
            .ShouldPass<bool>((because, alive) => because.ItsTrue("the tracked instance was collected", !alive))
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void ApplyGlobalVerbosityToLiveInstances()
    {
        When.A<ConstructionSensitive>("sets the global verbosity after constructing an instance",
            () => new ConstructionSensitive(),
            instance =>
            {
                VerbosityLevel before = instance.LogVerbosity;
                Loggable.SetGlobalVerbosity(VerbosityLevel.Warning);
                VerbosityLevel after = instance.LogVerbosity;
                Loggable.SetGlobalVerbosity(before);
                return after;
            })
            .TheTest
            .ShouldPass<VerbosityLevel>((because, level) => because.ItsTrue("the live instance took the level", level == VerbosityLevel.Warning))
            .SoBeHappy()
            .UnlessItFailed();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference Construct()
    {
        return new WeakReference(new ConstructionSensitive());
    }

    /// <summary>
    /// Like <c>Database</c>: equality and hashing read state the derived constructor sets, so they fail while the base
    /// constructor runs, and every instance is equal to every other.
    /// </summary>
    private sealed class ConstructionSensitive : Loggable
    {
        private readonly string _state;

        public ConstructionSensitive()
        {
            _state = "built";
        }

        public override bool Equals(object? obj)
        {
            return obj is ConstructionSensitive other && _state.Length == other._state.Length;
        }

        public override int GetHashCode()
        {
            return _state.Length;
        }
    }

    private sealed record ConcurrencyOutcome(string? Failure, int Missing);
}
