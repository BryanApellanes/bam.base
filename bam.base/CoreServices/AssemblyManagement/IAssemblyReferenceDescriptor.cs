using System.Collections.Generic;

namespace Bam.Net.CoreServices.AssemblyManagement.Data
{
    public interface IAssemblyReferenceDescriptor
    {
        List<IAssemblyDescriptor> AssemblyDescriptors { get; set; }
        string ReferencedHash { get; set; }
        string ReferencerHash { get; set; }
    }
}