using System.Collections.Generic;

namespace Bam.Net.CoreServices.AssemblyManagement.Data
{
    public interface IProcessRuntimeDescriptor
    {
        string ApplicationName { get; set; }
        IAssemblyDescriptor[] AssemblyDescriptors { get; set; }
        HashSet<string> AssemblyFileHashes { get; }
        string CommandLine { get; set; }
        string FilePath { get; set; }
        string MachineName { get; set; }
    }
}