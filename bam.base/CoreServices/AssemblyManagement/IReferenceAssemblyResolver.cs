using System;
using System.Reflection;

namespace Bam.Net.CoreServices.AssemblyManagement
{
    public interface IReferenceAssemblyResolver
    {
        Assembly ResolveReferenceAssembly(Type type);
        string ResolveReferenceAssemblyPath(Type type);
        string ResolveReferenceAssemblyPath(string nameSpace, string typeName);
        
        string ResolveReferenceAssemblyPath(string assemblyName);
    }
}