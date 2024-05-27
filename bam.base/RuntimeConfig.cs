//using Bam.Application;

namespace Bam
{
    public class RuntimeConfig
    {
        public const string File = "runtime-config.yaml";

        public RuntimeConfig() 
        {
            this.NugetPackageRoot = BamProfile.NugetPackagePath;
        }

        public string ReferenceAssemblies { get; set; }
        public string GenDir { get; set; }
        public string BamProfileDir { get; set; }
        public string BamDir { get; set; }
        public string ProcessProfileDir { get; set; }

        public string NugetPackageRoot { get; set; }
    }
}