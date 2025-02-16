using Bam.DependencyInjection;
using Bam.Services;

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
