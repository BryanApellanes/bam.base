using Bam.Net;
using Bam.Net.CoreServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public class ProcessModeServiceRegistry
    {
        public static implicit operator ServiceRegistry(ProcessModeServiceRegistry processModeServiceRegistry)
        {
            return processModeServiceRegistry.ServiceRegistry;
        }

        public ProcessModeServiceRegistry(ProcessModes processMode, ServiceRegistry serviceRegistry) 
        {
            this.ProcessMode = processMode;
            this.ServiceRegistry = serviceRegistry;
        }

        public ProcessModes ProcessMode
        {
            get;
        }

        public ServiceRegistry ServiceRegistry 
        {
            get;
            internal set;
        }
    }
}
