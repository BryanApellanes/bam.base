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
            (assembly) => new AssemblyFileOutcome(assembly.GetFileInfo(), assembly.GetFilePath(), assembly.Location, assembly.TryGetFileInfo(out FileInfo? _), assembly.GetFilePathOrName()))
        .TheTest
        .ShouldPass<AssemblyFileOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("the file exists", outcome.FileInfo.Exists);
            because.ItsTrue("the file is the assembly location", outcome.FileInfo.FullName.Equals(new FileInfo(outcome.Location).FullName));
            because.ItsTrue("GetFilePath agrees with GetFileInfo", outcome.FilePath.Equals(outcome.FileInfo.FullName));
            because.ItsTrue("TryGetFileInfo reports the file", outcome.TryFound);
            because.ItsTrue("GetFilePathOrName returns the path", outcome.PathOrName.Equals(outcome.FileInfo.FullName));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void ReportNoFileForAnAssemblyLoadedFromBytes()
    {
        When.A<Assembly>("was loaded from bytes, as runtime-generated Dao assemblies are",
            () => Assembly.Load(File.ReadAllBytes(typeof(ObsoleteApiReplacementsShould).Assembly.Location)),
            (assembly) =>
            {
                bool hasFile = assembly.TryGetFileInfo(out FileInfo? fileInfo);
                bool getFileInfoThrew = false;
                try
                {
                    assembly.GetFileInfo();
                }
                catch (InvalidOperationException)
                {
                    getFileInfoThrew = true;
                }
                return new BytesLoadedOutcome(hasFile, fileInfo, getFileInfoThrew, assembly.GetFilePathOrName(), assembly.FullName!);
            })
        .TheTest
        .ShouldPass<BytesLoadedOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("TryGetFileInfo reports no file", !outcome.HasFile && outcome.FileInfo == null);
            because.ItsTrue("GetFileInfo throws InvalidOperationException instead of returning another assembly's path", outcome.GetFileInfoThrew);
            because.ItsTrue("GetFilePathOrName falls back to the assembly's full name", outcome.PathOrName.Equals(outcome.FullName));
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
                        Thread.SpinWait(1);
                    }
                });
                bool startedInTime = false;
                bool threwWhileBackground = false;
                bool threwWhileRunning = false;
                try
                {
                    thread.Start();
                    startedInTime = started.Wait(5000);
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
                    try
                    {
                        thread.Abort();
                    }
                    catch (PlatformNotSupportedException)
                    {
                        threwWhileRunning = true;
                    }
                }
                finally
                {
                    // released on every path, so a spinning foreground thread can never keep the test host alive
                    release.Set();
                }
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
                return new AbortOutcome(startedInTime, threwWhileBackground, threwWhileRunning, finished, threwAfterFinish);
            })
        .TheTest
        .ShouldPass<AbortOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("the thread started", outcome.Started);
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
                bool firstStartedInTime = false;
                bool secondStarted = false;
                bool firstAliveAfterReplace = false;
                bool firstAliveAfterKill = false;
                NamedThread? first = null;
                try
                {
                    first = Exec.Start(name, () =>
                    {
                        firstStarted.Set();
                        while (!release.IsSet)
                        {
                            Thread.SpinWait(1);
                        }
                    });
                    firstStartedInTime = firstStarted.Wait(5000);
                    Exec.Start(name, () => secondRan.Set());
                    secondStarted = secondRan.Wait(5000);
                    firstAliveAfterReplace = first.Thread.IsAlive;
                    Exec.Kill(name);
                    firstAliveAfterKill = first.Thread.IsAlive;
                }
                finally
                {
                    // the thread Exec.Start creates is a foreground thread; release it on every path
                    release.Set();
                }
                bool firstFinished = first.Thread.Join(5000);
                return new ExecOutcome(firstStartedInTime, secondStarted, firstAliveAfterReplace, firstAliveAfterKill, firstFinished);
            })
        .TheTest
        .ShouldPass<ExecOutcome>((because, _, outcome) =>
        {
            because.ItsTrue("the first thread started", outcome.FirstStarted);
            because.ItsTrue("a second thread of the same name starts", outcome.SecondStarted);
            because.ItsTrue("the replaced thread keeps running", outcome.FirstAliveAfterReplace);
            because.ItsTrue("Kill does not stop a running thread", outcome.FirstAliveAfterKill);
            because.ItsTrue("the thread finishes on its own", outcome.FirstFinished);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    private sealed record AssemblyFileOutcome(FileInfo FileInfo, string FilePath, string Location, bool TryFound, string PathOrName);

    private sealed record BytesLoadedOutcome(bool HasFile, FileInfo? FileInfo, bool GetFileInfoThrew, string PathOrName, string FullName);

    private sealed record SecureCuidOutcome(HashSet<string> Cuids, string Slug);

    private sealed record AbortOutcome(bool Started, bool ThrewWhileBackground, bool ThrewWhileRunning, bool Finished, bool ThrewAfterFinish);

    private sealed record ExecOutcome(bool FirstStarted, bool SecondStarted, bool FirstAliveAfterReplace, bool FirstAliveAfterKill, bool FirstFinished);
}
