using Bam.CoreServices.AssemblyManagement;
using Microsoft.CodeAnalysis;

namespace Bam
{
    public class AggregateMetadataReferenceResolver : IMetadataReferenceResolver
    {
        public AggregateMetadataReferenceResolver(params IMetadataReferenceResolver[] resolvers) 
        {
            this.Resolvers = new HashSet<IMetadataReferenceResolver>(resolvers);
        }

        public HashSet<IMetadataReferenceResolver> Resolvers { get; private set; }

        public MetadataReference[] GetMetaDataReferences()
        {
            return Resolvers.SelectMany(r => r.GetMetaDataReferences()).ToArray();
        }
    }
}
