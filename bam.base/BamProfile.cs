using System;
using System.Collections.Generic;
using System.IO;

namespace Bam.Net
{
    /// <summary>
    /// Paths rooted in the current process' user profile.
    /// </summary>
    public static class BamProfile
    {
        static BamProfile()
        {
            EnsureDirectoryExists(ToolkitPath);
            EnsureDirectoryExists(NugetOutputPath);
            EnsureDirectoryExists(ConfigPath);
            EnsureDirectoryExists(TestsPath);
            EnsureDirectoryExists(ContentPath);
            EnsureDirectoryExists(AppsPath);
            EnsureDirectoryExists(SvcScriptsSrcPath);
            EnsureDirectoryExists(ProxiesPath);
            EnsureDirectoryExists(DataPath);
            EnsureDirectoryExists(FilesPath);
            EnsureDirectoryExists(VaultsPath);
            EnsureDirectoryExists(RecipesPath);
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        /// <summary>
        /// The path to the .bam directory in the home directory of the current process' user.
        /// This value is the same as BamHome.Profile.
        /// </summary>
        public static string Path => System.IO.Path.Combine(UserHome, ".bam");
        
        /// <summary>
        /// The path to the home directory of the current process' user.
        /// This value is the same as BamHome.UserHome.
        /// </summary>
        public static string UserHome
        {
            get
            {
                if (OSInfo.Current == OSNames.Windows)
                {
                    return Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                }
                else
                {
                    return Environment.GetEnvironmentVariable("HOME");
                }
            }
        }
                
        public static string ToolkitPath => System.IO.Path.Combine(ToolkitSegments);
        public static string[] ToolkitSegments => new string[] {Path, "toolkit"};
        public static string NugetOutputPath => System.IO.Path.Combine(NugetOutputSegments);
        
        public static string[] NugetOutputSegments => new string[] {Path, "nupkg"};
        
        /// <summary>
        /// ~/.bam/config
        /// </summary>
        public static string ConfigPath => System.IO.Path.Combine(ConfigSegments);
        public static string[] ConfigSegments => new string[] {Path, "config"};

        public static string TestsPath => System.IO.Path.Combine(TestsSegments);
        public static string[] TestsSegments => new string[] {Path, "tests"};
        public static string ContentPath => System.IO.Path.Combine(ContentSegments);
        public static string[] ContentSegments => new string[] {Path, "content"};

        public static string AppsPath => System.IO.Path.Combine(AppsSegments);
        public static string[] AppsSegments => new List<string>(ContentSegments) {"apps"}.ToArray();
        
        public static string SvcScriptsSrcPath => System.IO.Path.Combine(SvcScriptsSrcSegments);
        public static string[] SvcScriptsSrcSegments => new string[] {Path, "svc", "scripts"};

        public static string GeneratedPath => System.IO.Path.Combine(DataPath, "generated");
        public static string ProxiesPath => System.IO.Path.Combine(DataPath, "proxies");

        public static string DataPath => System.IO.Path.Combine(DataSegments);

        public static string[] DataSegments => new string[] {Path, "data"};

        public static string FilesPath => System.IO.Path.Combine(FilesSegments);
        public static string[] FilesSegments => new string[] {Path, "files"};

        public static string LogsPath => System.IO.Path.Combine(Path, "logs");

        public static string VaultsPath => System.IO.Path.Combine(VaultsSegments);
        public static string[] VaultsSegments => new string[] {Path, "vaults"};
        
        public static string RecipesPath => System.IO.Path.Combine(RecipesSegments);
        public static string[] RecipesSegments => new string[] {Path, "recipes"};

        public static string ScreenshotsPath => System.IO.Path.Combine(ScreenshotsSegments);
        public static string[] ScreenshotsSegments => new string[] {Path, "screenshots"};
        
        public static string ReadDataFile(string relativeFilePath)
        {
            FileInfo file = new FileInfo(System.IO.Path.Combine(DataPath, relativeFilePath));
            if (!file.Exists)
            {
                File.Create(file.FullName).Dispose();
            }

            return File.ReadAllText(file.FullName);
        }
        
        public static T LoadJsonData<T>(string relativeFilePath) where T : new()
        {
            FileInfo file = new FileInfo(System.IO.Path.Combine(DataPath, relativeFilePath));
            if (!file.Exists)
            {
                File.Create(file.FullName).Dispose();
            }
			T instance = file.FromJsonFile<T>();
			if (instance == null)
			{
				return default(T);
			}
			return instance;
        }
        
        public static T LoadYamlData<T>(string relativeFilePath) where T : new()
        {
            FileInfo file = new FileInfo(System.IO.Path.Combine(DataPath, relativeFilePath));
            if (!file.Exists)
            {
                File.Create(file.FullName).Dispose();
            }
			T instance = file.FromYamlFile<T>();
			if (instance == null)
			{
				return default(T);
			}
			return instance;
        }

        public static string SaveJsonData(object instance, string relativeFilePath)
        {
            FileInfo file = new FileInfo(System.IO.Path.Combine(DataPath, relativeFilePath));
            instance.ToJson().SafeWriteToFile(file.FullName, true);
            return file.FullName;
        }
        
        public static string SaveYamlData(object instance, string relativeFilePath)
        {
            FileInfo file = new FileInfo(System.IO.Path.Combine(DataPath, relativeFilePath));
            instance.ToYaml().SafeWriteToFile(file.FullName, true);
            return file.FullName;
        }
    }
}