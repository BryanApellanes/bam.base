using System;
using System.Reflection;
using System.Threading.Tasks;
using Bam.Net.CoreServices.AssemblyManagement.Data;

namespace Bam.Net.CoreServices
{
    public interface IAssemblyService: IAssemblyResolver
    {
        IProcessRuntimeDescriptor CurrentProcessRuntimeDescriptor { get; set; }
        IProcessRuntimeDescriptor LoadRuntimeDescriptor(IProcessRuntimeDescriptor likeThis);
        IProcessRuntimeDescriptor LoadRuntimeDescriptor(string filePath, string commandLine, string machineName, string applicationName);
        void RestoreApplicationRuntime(string applicationName, string directoryPath);
    }
}