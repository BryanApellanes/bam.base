using Bam;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
