﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Bam.CoreServices.AssemblyManagement.Data;

namespace Bam.CoreServices
{
    public interface IAssemblyService: IAssemblyResolver
    {
        IProcessRuntimeDescriptor CurrentProcessRuntimeDescriptor { get; set; }
        IProcessRuntimeDescriptor LoadRuntimeDescriptor(IProcessRuntimeDescriptor likeThis);
        IProcessRuntimeDescriptor LoadRuntimeDescriptor(string filePath, string commandLine, string machineName, string applicationName);
        void RestoreApplicationRuntime(string applicationName, string directoryPath);
    }
}