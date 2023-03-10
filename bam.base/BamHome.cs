using Bam.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bam.Net
{
    /// <summary>
    /// Paths rooted in the root of the bam installation. (/opt/bam on *nix, /c/bam on windows)
    /// </summary>
    public static class BamHome
    {
        public static string SystemRoot => OSInfo.IsWindows ? "c:/": "/";
        public static string Path => System.IO.Path.Combine(PathSegments);

        /// <summary>
        /// The root of the bam installation, the same as BamHome.Path.
        /// </summary>
        public static string Root => Path;

        public static string HostData => System.IO.Path.Combine(Local, Environment.MachineName);
        
        public static string Local => System.IO.Path.Combine(Path, "local");
        
        /// <summary>
        /// The path segments for BamHome, on Windows c:/opt/bam, otherwise /opt/bam
        /// </summary>
        public static string[] PathSegments
        {
            get
            {
                return new string[] {SystemRoot, "opt", "bam"};
            }
        }

        public static string Public => System.IO.Path.Combine(Path, "public");
        
        public static string Profile => BamProfile.Path;
        
        public static string UserHome => BamProfile.UserHome;

        public static string PublicPath => System.IO.Path.Combine(Path, "public");

        /// <summary>
        /// The default path where the bam toolkit should be found.  Same as BamProfile.ToolkitPath.
        /// </summary>
        public static string ToolkitPath => BamProfile.ToolkitPath;

        /// <summary>
        /// Path segments representing the path where the bam toolkit should be found.  Same as BamProfile.ToolkitSegments
        /// </summary>
        public static string[] ToolkitSegments => BamProfile.ToolkitSegments;

        public static string CurrentRuntimeToolkitPath => System.IO.Path.Combine(CurrentRuntimeToolkitSegments);
        
        public static string[] CurrentRuntimeToolkitSegments => new List<string>(ToolkitSegments) {OSInfo.CurrentRuntime}.ToArray();
        
        public static string NugetOutputPath => BamProfile.NugetOutputPath;
        
        public static string[] NugetOutputSegments => BamProfile.NugetOutputSegments;
        
        /// <summary>
        /// The path where third party tools are found, including sysinternals and opencover.
        /// </summary>
        public static string ToolsPath => System.IO.Path.Combine(ToolsSegments);
        public static string[] ToolsSegments => new List<string>() {Path, "bin", "tools"}.ToArray();
        
        public static string TestsPath => System.IO.Path.Combine(TestsSegments);

        public static string[] TestsSegments => new List<string>() {UserHome, ".bam", "tests"}.ToArray();

        public static string ContentPath => System.IO.Path.Combine(ContentSegments);

        public static string[] ContentSegments => new List<string>(PathSegments) {"content"}.ToArray();

        public static string AppsPath => System.IO.Path.Combine(AppsSegments);

        public static string[] AppsSegments => new List<string>(ContentSegments) {"apps"}.ToArray();

        public static string SvcScriptsSrcPath => System.IO.Path.Combine(SvcScriptsSrcSegments);

        public static string[] SvcScriptsSrcSegments => new List<string>(PathSegments) {"svc", "scripts"}.ToArray();

        public static string ConfigPath => System.IO.Path.Combine(ConfigSegments);

        public static string[] ConfigSegments => new List<string>(PathSegments) {"config"}.ToArray();

        public static string DataPath => System.IO.Path.Combine(DataSegments);

        public static string[] DataSegments => new List<string>(PathSegments) {"data"}.ToArray();
        
        public static string RecipesPath => System.IO.Path.Combine(RecipeSegments);
        public static string[] RecipeSegments => new List<string>(PathSegments) {"recipes"}.ToArray();
    }
}
