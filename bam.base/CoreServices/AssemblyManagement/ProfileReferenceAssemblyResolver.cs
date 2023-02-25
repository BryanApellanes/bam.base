using System;
using System.IO;

namespace Bam.Net.CoreServices.AssemblyManagement
{
    public class ProfileReferenceAssemblyResolver : NugetReferenceAssemblyResolver
    {
        public ProfileReferenceAssemblyResolver() : base(Path.Combine(RuntimeSettings.ProcessProfileDir, ".nuget", "packages"))
        {
        }
    }
}