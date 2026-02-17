//using Bam.Application;

namespace Bam
{
    public class RuntimeConfig
    {
        public const string FileName = "runtime-config.yaml";

        public RuntimeConfig() 
        {
            this.NugetPackageRoot = BamProfile.NugetPackagePath;
        }

        public string ReferenceAssemblies { get; set; } = null!;
        public string GenDir { get; set; } = null!;
        public string BamProfileDir { get; set; } = null!;
        public string BamDir { get; set; } = null!;
        public string ProcessProfileDir { get; set; } = null!;

        public string NugetPackageRoot { get; set; }

        public static FileInfo File => new(Path.Combine(RuntimeSettings.BamDir, RuntimeSettings.GetOsAlias(), FileName)); 
        
        public static RuntimeConfig Current
        {
            get
            {
                if (!File.Exists)
                {
                    WriteDefault();
                }

                return File.FromYamlFile<RuntimeConfig>();
            }
        }
        
        public static string WriteDefault(bool overwrite = false)
        {
            FileInfo runtimeConfigFile = File;
            if (runtimeConfigFile.Exists && overwrite == false)
            {
                return runtimeConfigFile.FullName;
            }

            RuntimeConfig config = new RuntimeConfig()
            {
                ReferenceAssemblies = RuntimeSettings.GetReferenceAssembliesDirectory(),
                GenDir = RuntimeSettings.GetGenDir(),
                BamProfileDir = RuntimeSettings.BamProfileDir,
                BamDir = RuntimeSettings.BamDir,
                ProcessProfileDir = RuntimeSettings.ProcessProfileDir!
            };
            config.ToYamlFile(runtimeConfigFile);
            
            return runtimeConfigFile.FullName;
        }
    }
}