
using System;
using System.IO;
using System.Reflection;
using Bam.Net.Logging;

namespace Bam.Net.CoreServices.AssemblyManagement
{
    public class LinuxReferenceAssemblyResolver : IReferenceAssemblyResolver
    {
        private const string _dotnetAssemblyPath = "/usr/share/dotnet/shared/Microsoft.NETCore.App";
        public LinuxReferenceAssemblyResolver()
        {
            NugetReferenceAssemblyResolver = new NugetReferenceAssemblyResolver(_dotnetAssemblyPath);
            RuntimeSettingsConfigReferenceAssemblyResolver = new RuntimeSettingsConfigReferenceAssemblyResolver();
        }

        private NugetReferenceAssemblyResolver NugetReferenceAssemblyResolver { get; set; }
        private RuntimeSettingsConfigReferenceAssemblyResolver RuntimeSettingsConfigReferenceAssemblyResolver { get; set; }
        public Assembly ResolveReferenceAssembly(Type type)
        {
            return Assembly.LoadFrom(ResolveReferenceAssemblyPath(type));
        }

        public string ResolveReferenceAssemblyPath(Type type)
        {
            FileInfo assemblyFile = new FileInfo(Path.Combine(_dotnetAssemblyPath, OSInfo.TargetFrameworkVersion, $"{type.Namespace}.{type.Name}.dll"));
            if (assemblyFile.Exists)
            {
                return assemblyFile.FullName;
            }
            throw new ReferenceAssemblyNotFoundException(type);
        }

        public string ResolveReferenceAssemblyPath(string nameSpace, string typeName)
        {
            return NugetReferenceAssemblyResolver.ResolveReferenceAssemblyPath(nameSpace, typeName);
        }

        public string ResolveReferenceAssemblyPath(string assemblyName)
        {
            return Path.Combine(RuntimeSettings.GetReferenceAssembliesDirectory(), assemblyName);
        }

        public string ResolveSystemRuntimePath()
        {
            return NugetReferenceAssemblyResolver.ResolveSystemRuntimePath();
        }
    }
}