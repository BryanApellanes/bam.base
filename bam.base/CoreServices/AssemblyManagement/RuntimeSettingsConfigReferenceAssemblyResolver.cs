using System;
using System.IO;
using System.Reflection;

namespace Bam.Net.CoreServices.AssemblyManagement
{
    public class RuntimeSettingsConfigReferenceAssemblyResolver : ReferenceAssemblyResolver
    {
        public override string ResolveReferenceAssemblyPath(Type type)
        {
            return ResolveReferenceAssemblyPath(type.Namespace, type.Name);
        }

        public override string ResolveReferenceAssemblyPath(string typeNamespace, string typeName)
        {
            string referenceAssembliesDir = RuntimeSettings.GetRuntimeConfig().ReferenceAssemblies;

            string filePath = FindAssembly(typeNamespace, typeName, referenceAssembliesDir);

            if (!File.Exists(filePath))
            {
                filePath = FindAssembly(typeNamespace, typeName, Assembly.GetEntryAssembly().GetFileInfo().Directory.FullName);
            }

            if (!File.Exists(filePath))
            {
                throw new ReferenceAssemblyNotFoundException($"{typeNamespace}.{typeName}");
            }

            return filePath;
        }

        private static string FindAssembly(string typeNamespace, string typeName, string referenceAssembliesDir)
        {
            string filePath = Path.Combine(referenceAssembliesDir, $"{typeNamespace}.dll");
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(referenceAssembliesDir, $"{typeNamespace}.{typeName}.dll");
            }

            return filePath;
        }
    }
}