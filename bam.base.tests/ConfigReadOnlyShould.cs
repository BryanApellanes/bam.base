using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

/// <summary>
/// Reading Bam configuration must not write to disk, and <c>BAM_HOME</c> moves the home root (bam.base#22).
/// </summary>
[UnitTestMenu("Config read-only should")]
public class ConfigReadOnlyShould : UnitTestMenuContainer
{
    public ConfigReadOnlyShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    private static string ScratchPath(string name)
    {
        return Path.Combine(Path.GetTempPath(), "bam-base-22", name + "-" + Guid.NewGuid().ToString("N"));
    }

    private static T WithBamHome<T>(string? value, Func<T> action)
    {
        string? previous = Environment.GetEnvironmentVariable(BamEnvironmentVariables.BAM_HOME);
        Environment.SetEnvironmentVariable(BamEnvironmentVariables.BAM_HOME, value);
        try
        {
            return action();
        }
        finally
        {
            Environment.SetEnvironmentVariable(BamEnvironmentVariables.BAM_HOME, previous);
        }
    }

    [UnitTest]
    public void ReadMissingConfigurationAsEmptyWithoutCreatingAnything()
    {
        string home = ScratchPath("missing-home");

        When.A<string>("reads configuration under a BAM_HOME that does not exist",
            () => home,
            root => WithBamHome(root, () =>
            {
                Dictionary<string, string> settings = Config.Read("probe-app", out FileInfo file);
                Config constructed = new Config("probe-app");
                FileInfo profileFile = Config.GetBamProfileConfigFile("probe-app-" + Guid.NewGuid().ToString("N"));
                return new MissingOutcome(
                    settings.Count,
                    constructed.AppSettings.Count,
                    file.FullName,
                    System.IO.File.Exists(file.FullName),
                    Directory.Exists(root),
                    System.IO.File.Exists(profileFile.FullName));
            }))
            .TheTest
            .ShouldPass<MissingOutcome>((because, outcome) =>
            {
                because.ItsTrue("a missing file reads as empty", outcome.ReadCount == 0);
                because.ItsTrue("a Config built over it is empty", outcome.ConstructedCount == 0);
                because.ItsTrue("the file is looked for under BAM_HOME", outcome.FilePath.StartsWith(Path.GetFullPath(home), StringComparison.OrdinalIgnoreCase));
                because.ItsTrue("the file is not created", !outcome.FileCreated);
                because.ItsTrue("no directory is created under BAM_HOME", !outcome.HomeCreated);
                because.ItsTrue("the profile config file is not created either", !outcome.ProfileFileCreated);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void MoveTheHomeRootWithBamHome()
    {
        string home = ScratchPath("moved-home");

        When.A<string>("resolves BamHome with and without BAM_HOME",
            () => home,
            root => new HomeOutcome(
                WithBamHome(root, () => BamHome.Path),
                WithBamHome(root, () => BamHome.ConfigPath),
                WithBamHome("relative/home", () => BamHome.Path),
                WithBamHome(null, () => BamHome.Path)))
            .TheTest
            .ShouldPass<HomeOutcome>((because, outcome) =>
            {
                string expectedDefault = Path.Combine(BamHome.SystemRoot, "opt", "bam");
                because.ItsTrue("BAM_HOME becomes the home root", outcome.Moved == Path.GetFullPath(home));
                because.ItsTrue("the config path is under it", outcome.MovedConfig == Path.Combine(Path.GetFullPath(home), "config"));
                because.ItsTrue("a relative BAM_HOME is ignored", outcome.Relative == expectedDefault);
                because.ItsTrue("without BAM_HOME the root is /opt/bam", outcome.Default == expectedDefault);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void ReadAndSaveAnExistingConfigFileAsBefore()
    {
        string file = Path.Combine(ScratchPath("existing"), "app", "app.appsettings.yaml");

        When.A<string>("saves configuration to a new file and reads it back",
            () => file,
            path =>
            {
                Config config = new Config("app", path);
                bool existedBeforeSave = System.IO.File.Exists(path);
                config.AppSettings["Greeting"] = "hello";
                config.Save();
                Dictionary<string, string> reread = Config.Read(new FileInfo(path));
                return new SaveOutcome(existedBeforeSave, System.IO.File.Exists(path), reread.TryGetValue("Greeting", out string? greeting) ? greeting : null);
            })
            .TheTest
            .ShouldPass<SaveOutcome>((because, outcome) =>
            {
                because.ItsTrue("constructing the Config does not create the file", !outcome.ExistedBeforeSave);
                because.ItsTrue("saving creates the file and its directory", outcome.ExistsAfterSave);
                because.ItsTrue("the saved value reads back", outcome.Greeting == "hello");
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    [UnitTest]
    public void TraceRatherThanThrowWhenAProfileDirectoryCannotBeCreated()
    {
        string blocker = ScratchPath("blocker");

        When.A<string>("asks for a profile directory beneath a file",
            () => blocker,
            path =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                System.IO.File.WriteAllText(path, "a file where a directory would go");
                bool created = BamProfile.EnsureDirectoryExists(Path.Combine(path, "child"));
                bool existing = BamProfile.EnsureDirectoryExists(Path.GetDirectoryName(path)!);
                return new ProfileOutcome(created, existing);
            })
            .TheTest
            .ShouldPass<ProfileOutcome>((because, outcome) =>
            {
                because.ItsTrue("an uncreatable directory reports false instead of throwing", !outcome.Created);
                because.ItsTrue("an existing directory reports true", outcome.Existing);
            })
            .SoBeHappy()
            .UnlessItFailed();
    }

    private sealed record MissingOutcome(int ReadCount, int ConstructedCount, string FilePath, bool FileCreated, bool HomeCreated, bool ProfileFileCreated);

    private sealed record HomeOutcome(string Moved, string MovedConfig, string Relative, string Default);

    private sealed record SaveOutcome(bool ExistedBeforeSave, bool ExistsAfterSave, string? Greeting);

    private sealed record ProfileOutcome(bool Created, bool Existing);
}
