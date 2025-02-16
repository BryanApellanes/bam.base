using Microsoft.CodeAnalysis;

namespace Bam.CoreServices.AssemblyManagement
{
    public interface IMetadataReferenceResolver
    {
        MetadataReference[] GetMetaDataReferences();
    }
}
