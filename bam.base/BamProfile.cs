namespace Bam
{
    /// <summary>
    /// Paths rooted in the .bam direcotry of the owner of the current process' user profile.
    /// </summary>
    public static class BamProfile
    {
        static BamProfile()
        {
            EnsureDirectoryExists(ToolkitPath);
            EnsureDirectoryExists(NugetPackagePath);
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
                    return Environment.GetEnvironmentVariable("USERPROFILE") ?? System.IO.Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE"), Environment.GetEnvironmentVariable("HOMEPATH"));
                }
                else
                {
                    return Environment.GetEnvironmentVariable("HOME");
                }
            }
        }

        public static string DotSys(string rootPath)
        {
            return System.IO.Path.Combine(rootPath, ".sys");
        }
        
        public static string ToolkitPath => System.IO.Path.Combine(ToolkitSegments);
        public static string[] ToolkitSegments => new string[] {Path, "toolkit"};
        public static string NugetPackagePath => System.IO.Path.Combine(NugetPackageSegments);
        
        public static string[] NugetPackageSegments => new string[] {Path, "nupkg"};
        
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

        public static string DataDotSys => DotSys(DataPath);
        public static string DataPath => System.IO.Path.Combine(DataSegments);

        public static string[] DataSegments => new string[] {Path, "data"};

        public static string FilesPath => System.IO.Path.Combine(FilesSegments);
        public static string[] FilesSegments => new string[] {Path, "files"};

        public static string LogsPath => System.IO.Path.Combine(Path, "logs");

        public static string VaultsDotSys => DotSys(VaultsPath);
        public static string VaultsPath => System.IO.Path.Combine(VaultsSegments);
        public static string[] VaultsSegments => new string[] {Path, "vaults"};
        
        public static string RecipesPath => System.IO.Path.Combine(RecipesSegments);
        public static string[] RecipesSegments => new string[] {Path, "recipes"};

        public static string ScreenshotsPath => System.IO.Path.Combine(ScreenshotsSegments);
        public static string[] ScreenshotsSegments => new string[] {Path, "screenshots"};

        /// <summary>
        /// Creates or overwrites a file named <paramref name="fileName"/> in the VaultsDotSys directory with the
        /// specified content.
        /// </summary>
        /// <remarks>If a file with the specified name already exists in the VaultsDotSys directory, it
        /// will be overwritten. The method writes the entire content to the file in a single operation.</remarks>
        /// <param name="fileName">The name of the file to create or overwrite within the VaultsDotSys directory. Cannot be null or empty.</param>
        /// <param name="content">The byte array containing the data to write to the file. Cannot be null.</param>
        /// <returns>A <see cref="FileInfo"/> object representing the file that was written.</returns>
        public static FileInfo WriteVaultDotSysFile(string fileName, byte[] content)
        {
            FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(VaultsDotSys, fileName));
            fileInfo.FullName.SafeWriteFile(content, true);
            return fileInfo;
        }

        /// <summary>
        /// Creates or overwrites a file with the specified name in the VaultsDotSys directory and writes the provided
        /// content to it.
        /// </summary>
        /// <param name="fileName">The name of the file to create or overwrite within the VaultsDotSys directory. Cannot be null or empty.</param>
        /// <param name="content">The text content to write to the file. If the file already exists, its contents are replaced.</param>
        /// <returns>A FileInfo object representing the file that was created or overwritten.</returns>
        public static FileInfo WriteVaultDotSysFile(string fileName, string content)
        {
            FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(VaultsDotSys, fileName));
            fileInfo.FullName.SafeWriteFile(content, true);
            return fileInfo;
        }

        /// <summary>
        /// Attempts to read the contents of a Vault.sys file as a string.
        /// </summary>
        /// <remarks>This method does not throw exceptions if the file cannot be read. Instead, it returns
        /// false and sets the out parameter to null.</remarks>
        /// <param name="fileName">The full path to the Vault.sys file to read. Cannot be null or empty.</param>
        /// <param name="content">When this method returns, contains the contents of the file as a string if the operation succeeded;
        /// otherwise, null.</param>
        /// <returns>true if the file was read successfully; otherwise, false.</returns>
        public static bool TryReadVaultDotSysFileString(string fileName, out string? content)
        {
            try
            {
                content = ReadVaultDotSysFileString(fileName);
                return true;
            }
            catch (Exception ex) when (ex is not StackOverflowException && ex is not OutOfMemoryException)
            {
                content = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to read the contents of a Vault.sys file and returns the result as a byte array.
        /// </summary>
        /// <remarks>This method does not throw exceptions if the file cannot be read. Instead, it returns
        /// false and sets the out parameter to null. Use this method when you want to handle file read failures without
        /// exceptions.</remarks>
        /// <param name="fileName">The full path to the Vault.sys file to read. Cannot be null or empty.</param>
        /// <param name="content">When this method returns, contains the file contents as a byte array if the operation succeeds; otherwise,
        /// null.</param>
        /// <returns>true if the file was read successfully; otherwise, false.</returns>
        public static bool TryReadVaultDotSysFileBytes(string fileName, out byte[]? content)
        {
            try
            {
                content = ReadVaultDotSysFileBytes(fileName);
                return true;
            }
            catch (Exception ex) when (ex is not StackOverflowException && ex is not OutOfMemoryException)
            {
                content = null;
                return false;
            }
        }

        /// <summary>
        /// Reads the contents of a file with the specified name from the VaultsDotSys directory as a string.
        /// </summary>
        /// <param name="fileName">The name of the file to read from the VaultsDotSys directory. Cannot be null or empty.</param>
        /// <returns>A string containing the entire contents of the specified file.</returns>
        public static string ReadVaultDotSysFileString(string fileName)
        {
            return File.ReadAllText(System.IO.Path.Combine(VaultsDotSys, fileName));
        }

        /// <summary>
        /// Reads all bytes from the specified file located in the VaultsDotSys directory.
        /// </summary>
        /// <param name="fileName">The name of the file to read from the VaultsDotSys directory. Cannot be null or empty.</param>
        /// <returns>A byte array containing the contents of the specified file.</returns>
        public static byte[] ReadVaultDotSysFileBytes(string fileName)
        {
            return File.ReadAllBytes(System.IO.Path.Combine(VaultsDotSys, fileName));
        }

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