using System.Reflection;
using Bam.DependencyInjection;
using Bam.Test;
using NCuid;

namespace Bam.Tests;

/// <summary>
/// Pins the behavior of the members that stopped calling obsolete runtime APIs (bam.base#44):
/// Assembly.CodeBase, Thread.Abort and RNGCryptoServiceProvider.
/// </summary>
[UnitTestMenu("Obsolete API replacements should")]
public class ObsoleteApiReplacementsShould : UnitTestMenuContainer
{
    public ObsoleteApiReplacementsShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void ResolveTheAssemblyFile()
    {
        When.A<Assembly>("is asked for its file",
            typeof(ObsoleteApiReplacementsShould).Assembly,
            (assembly) => new AssemblyFileOutcome(assembly.GetFileInfo(), assembly.GetFilePath(), assembly.Location))
        .TheTest
        .ShouldPass<AssemblyFileOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("the file exists", outcome.FileInfo.Exists);
            because.ItsTrue("the file is the assembly location", outcome.FileInfo.FullName.Equals(new FileInfo(outcome.Location).FullName));
            because.ItsTrue("GetFilePath agrees with GetFileInfo", outcome.FilePath.Equals(outcome.FileInfo.FullName));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void GenerateSecureCuidsAndSlugs()
    {
        When.A<HashSet<string>>("collects cuids from the secure random source",
            new HashSet<string>(),
            (cuids) =>
            {
                for (int i = 0; i < 200; i++)
                {
                    cuids.Add(Cuid.Generate(RandomSource.Secure));
                }
                return new SecureCuidOutcome(cuids, Cuid.Slug(RandomSource.Secure));
            })
        .TheTest
        .ShouldPass<SecureCuidOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("every cuid is unique", outcome.Cuids.Count == 200);
            because.ItsTrue("every cuid starts with c and is 25 characters", outcome.Cuids.All(cuid => cuid.StartsWith("c") && cuid.Length == 25));
            because.ItsTrue("a slug is produced", outcome.Slug.Length >= 7 && outcome.Slug.Length <= 10);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void RefuseToAbortARunningNamedThread()
    {
        When.A<ManualResetEventSlim>("aborts a named thread that is still running",
            new ManualResetEventSlim(false),
            (release) =>
            {
                ManualResetEventSlim started = new ManualResetEventSlim(false);
                NamedThread thread = new NamedThread($"abort-{Guid.NewGuid():N}", () =>
                {
                    started.Set();
                    while (!release.IsSet)
                    {
                        // spin: the thread has to be in the Running state, not WaitSleepJoin
                    }
                });
                bool threwWhileBackground = false;
                thread.Start();
                started.Wait(5000);
                try
                {
                    thread.Abort();
                }
                catch (PlatformNotSupportedException)
                {
                    threwWhileBackground = true;
                }
                // NamedThread starts background threads, whose state is Background, never exactly Running.
                thread.Thread.IsBackground = false;
                bool threwWhileRunning = false;
                try
                {
                    thread.Abort();
                }
                catch (PlatformNotSupportedException)
                {
                    threwWhileRunning = true;
                }
                release.Set();
                bool finished = thread.Thread.Join(5000);
                bool threwAfterFinish = false;
                try
                {
                    thread.Abort();
                }
                catch (PlatformNotSupportedException)
                {
                    threwAfterFinish = true;
                }
                return new AbortOutcome(threwWhileBackground, threwWhileRunning, finished, threwAfterFinish);
            })
        .TheTest
        .ShouldPass<AbortOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("aborting a running background thread does nothing", !outcome.ThrewWhileBackground);
            because.ItsTrue("aborting a running foreground thread throws PlatformNotSupportedException", outcome.ThrewWhileRunning);
            because.ItsTrue("the thread was left to finish on its own", outcome.Finished);
            because.ItsTrue("aborting a finished thread does nothing", !outcome.ThrewAfterFinish);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void LeaveARunningThreadAloneWhenKilledOrReplaced()
    {
        When.A<ManualResetEventSlim>("kills and replaces a running Exec thread",
            new ManualResetEventSlim(false),
            (release) =>
            {
                string name = $"exec-{Guid.NewGuid():N}";
                ManualResetEventSlim firstStarted = new ManualResetEventSlim(false);
                ManualResetEventSlim secondRan = new ManualResetEventSlim(false);
                NamedThread first = Exec.Start(name, () =>
                {
                    firstStarted.Set();
                    while (!release.IsSet)
                    {
                    }
                });
                firstStarted.Wait(5000);
                NamedThread second = Exec.Start(name, () => secondRan.Set());
                bool secondStarted = secondRan.Wait(5000);
                bool firstAliveAfterReplace = first.Thread.IsAlive;
                Exec.Kill(name);
                bool firstAliveAfterKill = first.Thread.IsAlive;
                release.Set();
                bool firstFinished = first.Thread.Join(5000);
                return new ExecOutcome(secondStarted, firstAliveAfterReplace, firstAliveAfterKill, firstFinished);
            })
        .TheTest
        .ShouldPass<ExecOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("a second thread of the same name starts", outcome.SecondStarted);
            because.ItsTrue("the replaced thread keeps running", outcome.FirstAliveAfterReplace);
            because.ItsTrue("Kill does not stop a running thread", outcome.FirstAliveAfterKill);
            because.ItsTrue("the thread finishes on its own", outcome.FirstFinished);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private sealed record AssemblyFileOutcome(FileInfo FileInfo, string FilePath, string Location);

    private sealed record SecureCuidOutcome(HashSet<string> Cuids, string Slug);

    private sealed record AbortOutcome(bool ThrewWhileBackground, bool ThrewWhileRunning, bool Finished, bool ThrewAfterFinish);

    private sealed record ExecOutcome(bool SecondStarted, bool FirstAliveAfterReplace, bool FirstAliveAfterKill, bool FirstFinished);
}
