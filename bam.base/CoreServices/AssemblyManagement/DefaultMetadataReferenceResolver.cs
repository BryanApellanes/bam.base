using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Bam.CoreServices.AssemblyManagement
{
    public class DefaultMetadataReferenceResolver : IMetadataReferenceResolver
    {
        public MetadataReference[] GetMetaDataReferences()
        {
            HashSet<MetadataReference> references = new HashSet<MetadataReference>
            {   
                // Get the path to the mscorlib and private mscorlib
                // libraries that are required for compilation to succeed.
                MetadataReference.CreateFromFile(RuntimeSettings.GetReferenceAssembliesDirectory() + Path.DirectorySeparatorChar + "mscorlib.dll"),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(RuntimeSettings.GetReferenceAssembliesDirectory() + Path.DirectorySeparatorChar + "netstandard.dll")
            };
            return references.ToArray();
        }
    }
}
