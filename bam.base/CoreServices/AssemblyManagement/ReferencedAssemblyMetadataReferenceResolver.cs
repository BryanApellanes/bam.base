using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Bam.CoreServices.AssemblyManagement
{
    public class ReferencedAssemblyMetadataReferenceResolver : IMetadataReferenceResolver
    {
        public MetadataReference[] GetMetaDataReferences()
        {
            HashSet<MetadataReference> references = new HashSet<MetadataReference>();
            Assembly? entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                AssemblyName[] referencedAssemblies = entryAssembly.GetReferencedAssemblies();
                foreach (AssemblyName referencedAssembly in referencedAssemblies)
                {
                    Assembly loadedAssembly = Assembly.Load(referencedAssembly);

                    references.Add(MetadataReference.CreateFromFile(loadedAssembly.Location));
                }
            }
            return references.ToArray();
        }
    }
}
