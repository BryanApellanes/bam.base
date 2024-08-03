using System.Reflection;

namespace Bam.CoreServices
{
    public interface IAssemblyResolver
    {
        Assembly ResolveAssembly(string assemblyName, string? assemblyDirectory = null);
    }
}