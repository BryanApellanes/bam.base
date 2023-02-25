//using Bam.Net.Application;

namespace Bam.Net
{
    public class RuntimeConfig
    {
        public const string File = "runtime-config.yaml";

        public string ReferenceAssemblies { get; set; }
        public string GenDir { get; set; }
        public string BamProfileDir { get; set; }
        public string BamDir { get; set; }
        public string ProcessProfileDir { get; set; }
    }
}