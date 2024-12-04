using Bam.CoreServices.AssemblyManagement;
using Microsoft.CodeAnalysis;

namespace Bam;

public class AssemblyPathMetadataReferenceResolver : IMetadataReferenceResolver
{
    public AssemblyPathMetadataReferenceResolver(params string[] referenceAssemblies)
    {
        _assemblyPaths = new List<string>();
        _assemblyPaths.AddRange(referenceAssemblies);
    }

    public void AddAssembly(string path)
    {
        _assemblyPaths.Add(path);
    }

    private readonly List<string> _assemblyPaths;

    public string[] AssemblyPaths => _assemblyPaths.ToArray();

    public MetadataReference[] GetMetaDataReferences()
    {
        List<MetadataReference> references = new List<MetadataReference>();
        foreach(string assemblyFilePath in AssemblyPaths)
        {
            references.Add(MetadataReference.CreateFromFile(assemblyFilePath));
        }

        return references.ToArray();
    }
}