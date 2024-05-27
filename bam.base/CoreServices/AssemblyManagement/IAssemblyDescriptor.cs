using Bam.Data.Repositories;
using System.Collections.Generic;

namespace Bam.CoreServices.AssemblyManagement.Data
{
    public interface IAssemblyDescriptor
    {
        string AssemblyFullName { get; set; }
        List<IAssemblyReferenceDescriptor> AssemblyReferenceDescriptors { get; set; }
        string FileHash { get; set; }
        string Name { get; set; }
        List<IProcessRuntimeDescriptor> ProcessRuntimeDescriptors { get; set; }

        bool Equals(object obj);
        int GetHashCode();
        IAssemblyDescriptor Save(IRepository repo);
    }
}