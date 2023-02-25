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

namespace Bam.Net
{
    public static partial class RuntimeSettings
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

        static string _appDataFolder;
        static readonly object _appDataFolderLock = new object();

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

        static Dictionary<OSNames, string> _osAliases = new Dictionary<OSNames, string>()
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

        static Dictionary<OSNames, string> _genDirs = new Dictionary<OSNames, string>()
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
        
        static Dictionary<OSNames, string> _referenceAssemblies = new Dictionary<OSNames, string>()
        {
            { OSNames.Windows, "C:\\Program Files\\dotnet\\packs\\Microsoft.NETCore.App.Ref\\5.0.0\\ref\\net5.0" },
            { OSNames.Linux, "/usr/share/dotnet/packs/Microsoft.NETCore.App.Ref/5.0.0/ref/net5.0" },
            { OSNames.OSX, "/usr/local/share/dotnet/packs/Microsoft.NETCore.App.Ref/5.0.0/ref/net5.0" }
        };

        public static string GetReferenceAssembliesDirectory()
        {
            return GetReferenceAssembliesDirectory(OSInfo.Current);
        }

        public static string GetReferenceAssembliesDirectory(OSNames osNames)
        {
            return _referenceAssemblies[osNames];
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
        public static string ProcessProfileDir => IsUnix ? Environment.GetEnvironmentVariable("HOME") : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

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
        
        public static string GetEntryDirectoryFilePathFor(string fileName)
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
