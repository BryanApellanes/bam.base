using Bam.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bam
{
    public static class RuntimeSettings
    {
        static RuntimeConfig _runtimeConfig;
        static object _runtimeConfigLock = new object();
        public static RuntimeConfig GetRuntimeConfig()
        {
            return _runtimeConfigLock.DoubleCheckLock(ref _runtimeConfig, () =>
            {
                FileInfo runtimeConfigFile = new FileInfo(Path.Combine(BamDir, GetOsAlias(), RuntimeConfig.File));
                if (runtimeConfigFile.Exists)
                {
                    return runtimeConfigFile.FromYamlFile<RuntimeConfig>();
                }

                RuntimeConfig config = new RuntimeConfig
                {
                    ReferenceAssemblies = GetReferenceAssembliesDirectory(),
                    GenDir = GetGenDir(),
                    BamProfileDir = BamProfileDir,
                    BamDir = BamDir,
                    ProcessProfileDir = ProcessProfileDir
                };
                config.ToYamlFile(runtimeConfigFile);
                _runtimeConfig = config;

                return _runtimeConfig;
            }); 
        }

        static string _processDataFolder;
        static readonly object _processDataFolderLock = new object();

        public static string ProcessDataFolder
        {
            get
            {
                return _processDataFolderLock.DoubleCheckLock(ref _processDataFolder, () =>
                {
                    string appName = DefaultConfiguration.GetAppSetting("ApplicationName", ApplicationNameProvider.Default.GetApplicationName());

                    string path = Path.Combine(BamProfile.AppsPath, appName);
                    Directory.CreateDirectory(path);
                    return path;
                });
            }
            set => _processDataFolder = value;
        }

        /// <summary>
        /// Gets a function that returns true if the specified type is not abstract, does not have the
        /// CompilerGeneratedAttribute and is not NestedPrivate, Sealed, Serializable or BeforeFieldInit.
        /// </summary>
        public static Func<Type, bool> ClrTypeFilter
        {
            get
            {
                return (t) => !t.IsAbstract && !t.HasCustomAttributeOfType<CompilerGeneratedAttribute>()
                              && t.Attributes != (
                                      TypeAttributes.NestedPrivate |
                                      TypeAttributes.Sealed |
                                      TypeAttributes.Serializable |
                                      TypeAttributes.BeforeFieldInit
                                  );
            }
        }

        static readonly Dictionary<OSNames, string> _osAliases = new Dictionary<OSNames, string>()
        {
            { OSNames.Windows, "win" },
            { OSNames.Linux, "lin" },
            { OSNames.OSX, "osx" },
        };

        public static string GetOsAlias()
        {
            return _osAliases[OSInfo.Current];
        }

        public static FileInfo EntryExecutable => Assembly.GetEntryAssembly().GetFileInfo();
        public static DirectoryInfo EntryDirectory => EntryExecutable.Directory;

        static readonly Dictionary<OSNames, string> _genDirs = new Dictionary<OSNames, string>()
        {
            { OSNames.Windows, WinGenDir },
            { OSNames.Linux, LinGenDir },
            { OSNames.OSX, OsxGenDir }
        };

        public static string GetGenDir()
        {
            return _genDirs[OSInfo.Current];
        }

        public static string GenDir => Path.Combine(BamDir, "gen");

        public static string WinGenDir => Path.Combine(GenDir, "win");
        public static string LinGenDir => Path.Combine(GenDir, "lin");
        public static string OsxGenDir => Path.Combine(GenDir, "osx");
        
        static readonly Dictionary<OSNames, string> _referenceAssemblyRootDirectories = new Dictionary<OSNames, string>()
        {
            { OSNames.Windows, "C:\\Program Files\\dotnet\\packs\\Microsoft.NETCore.App.Ref" }, 
            { OSNames.Linux, "/usr/share/dotnet/packs/Microsoft.NETCore.App.Ref" }, 
            { OSNames.OSX, "/usr/local/share/dotnet/packs/Microsoft.NETCore.App.Ref" } 
        };

        public static string ResolveReferenceAssemblyPathOrDie(string assemblyFileName)
        {
            string filePath = ResolveReferenceAssemblyPath(assemblyFileName);
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"The specified assembly was not found ({assemblyFileName}): {filePath}");
            }

            return filePath;
        }

        public static string ResolveReferenceAssemblyPath(string assemblyFileName)
        {
            if (assemblyFileName == null)
            {
                throw new ArgumentNullException(nameof(assemblyFileName));
            }
            if (!assemblyFileName.EndsWith(".dll"))
            {
                assemblyFileName = $"{assemblyFileName}.dll";
            }
            return Path.Join(GetReferenceAssembliesDirectory(), assemblyFileName);
        }

        public static string GetReferenceAssembliesDirectory()
        {
            return GetReferenceAssembliesDirectory(OSInfo.Current);
        }

        public static string GetReferenceAssembliesDirectory(OSNames osName)
        {
            string root = _referenceAssemblyRootDirectories[osName];
            string version = GetLatestInstalledDotNetVersion(osName);
            string refRoot = Path.Join(root, version, "ref");
            DirectoryInfo refRootDirectory = new DirectoryInfo(refRoot);
            string subFolder = refRootDirectory.GetDirectories().First().Name;
            return Path.Join(refRoot, subFolder);
        }

        public static string GetLatestInstalledDotNetVersion()
        {
            return GetLatestInstalledDotNetVersion(OSInfo.Current);
        }

        public static string[] GetInstalledDotNetVersions()
        {
            return GetInstalledDotNetVersions(OSInfo.Current);
        }

        private static string GetLatestInstalledDotNetVersion(OSNames osName)
        {
            string[] versions = GetInstalledDotNetVersions(osName);
            return versions[versions.Length - 1];
        }

        private static string[] GetInstalledDotNetVersions(OSNames osName)
        {
            DirectoryInfo root = new DirectoryInfo(_referenceAssemblyRootDirectories[osName]);
            List<string> versions = root.GetDirectories().Select(d => d.Name).ToList();
            versions.Sort();
            return versions.ToArray();
        }

        /// <summary>
        /// The path to the '.bam' directory found in the home directory of the owner of the
        /// current process.
        /// </summary>
        public static string BamProfileDir => Path.Combine(ProcessProfileDir, ".bam");

        /// <summary>
        /// The path to the the '.bam' directory found in the current working directory. 
        /// </summary>
        public static string BamDir => Path.Combine(Environment.CurrentDirectory, ".bam");

        /// <summary>
        /// The path to the home directory of the user that owns the current process.
        /// </summary>
        public static string? ProcessProfileDir => IsUnix ? Environment.GetEnvironmentVariable("HOME") : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

        /// <summary>
        /// Gets a value indicating if the current process is running on Windows.
        /// </summary>
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Gets a value indicating if the current process is running on a unix platform such as, Linux, BSD or Mac OSX.
        /// </summary>
        public static bool IsUnix => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// Gets a value indicating if the current runtime environment is a mac, same as IsOSX
        /// </summary>
        public static bool IsMac => IsOSX;

        /// <summary>
        /// Gets a value indicating if the current runtime environment is a mac, same as IsMac
        /// </summary>
        public static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        
        public static string GetEntryAssemblyDirectoryFilePathFor(string fileName)
        {
            Assembly entry = Assembly.GetEntryAssembly();
            FileInfo file = entry.GetFileInfo();
            DirectoryInfo directoryInfo = file.Directory;
            string directory = directoryInfo == null ? "." : directoryInfo.FullName;
            string result = Path.Combine(directory, fileName);
            return result;
        }
    }
}
