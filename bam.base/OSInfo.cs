using Bam.Net.CommandLine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Bam.Net.Logging;

namespace Bam.Net
{
    public class OSInfo
    {
        public const string DefaultTargetFrameworkVersion = "5.0.100";
        public const string DefaultLibSubfolder = "net5.0";
        
        static OSNames _current;
        public static OSNames Current
        {
            get
            {
                if (_current == OSNames.Invalid)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _current = OSNames.Windows;
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        _current = OSNames.Linux;
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        _current = OSNames.OSX;
                    }
                }

                return _current;
            }
        }

        public static bool IsUnix => Current != OSNames.Windows;
        public static bool IsWindows => Current == OSNames.Windows;

        public static string TargetFrameworkVersion => Config.Current["TargetFramework"].Or(DefaultTargetFrameworkVersion);

        private static readonly Dictionary<OSNames, string> _referenceRuntimeNames = new Dictionary<OSNames, string>
        {
            {OSNames.Invalid, "linux-x64"},
            {OSNames.Windows, "win-x64"},
            {OSNames.Linux, "linux-x64"},
            {OSNames.OSX, "osx-x64"}
        };

        public static string CurrentRuntime => BuildRuntimeName;

        public static string ReferenceRuntime => _referenceRuntimeNames[Current];

        static readonly Dictionary<OSNames, string> _buildRuntimeNames = new Dictionary<OSNames, string>
        {
            {OSNames.Invalid, "ubuntu.16.10-x64"},
            {OSNames.Windows, "win10-x64"},
            {OSNames.Linux, "ubuntu.16.10-x64"},
            {OSNames.OSX, "osx-x64"}
        };
        
        public static string BuildRuntimeName => _buildRuntimeNames[Current];

        public static string DefaultToolPath(string fileName)
        {
            return Current == OSNames.Windows ? $"c:/opt/bam/tools/{fileName}" : $"/opt/bam/tools/{fileName}";
        }
        
        /// <summary>
        /// Resolves the path to the specified executable file using 'which' or 'where' depending on the operating system
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool TryGetPath(string fileName, out string path)
        {
            try
            {
                path = GetPath(fileName);
                Log.Info("Found file {0}: {1}", fileName, path);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Exception finding path for {0}: {1}", fileName, ex.Message);
                path = string.Empty;
                return false;
            }
        }
        
        public static string GetPath(string fileName)
        {
            if (Current == OSNames.Windows)
            {
                ProcessOutput whereOutput = $"where {fileName}".Run();
                return ReadLastLine(whereOutput.StandardOutput);
            }
            ProcessOutput whichOutput = $"which {fileName}".Run();
            return ReadLastLine(whichOutput.StandardOutput);
        }

        private static string ReadLastLine(string output)
        {
            string[] lines = output.DelimitSplit("\r", "\n");
            if (lines.Length > 1)
            {
                return lines[0];
            }
            return lines[0];
        }
    }
}
