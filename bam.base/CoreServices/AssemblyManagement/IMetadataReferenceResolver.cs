using Bam.Net.CoreServices.AssemblyManagement;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.CoreServices.AssemblyManagement
{
    public interface IMetadataReferenceResolver
    {
        MetadataReference[] GetMetaDataReferences();
    }
}
