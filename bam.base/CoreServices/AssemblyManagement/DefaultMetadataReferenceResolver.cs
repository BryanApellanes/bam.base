using Bam.Net;
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
            List<MetadataReference> references = new List<MetadataReference>
            {   
                // Get the path to the mscorlib and private mscorlib
                // libraries that are required for compilation to succeed.
                MetadataReference.CreateFromFile(RuntimeSettings.GetReferenceAssembliesDirectory() + Path.DirectorySeparatorChar + "mscorlib.dll"),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(RuntimeSettings.GetReferenceAssembliesDirectory() + Path.DirectorySeparatorChar + "netstandard.dll")
            };
            Assembly? entryAssembly = Assembly.GetEntryAssembly();
            if(entryAssembly != null)
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
