using Bam.Net;
using Bam.Net.CoreServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public class BamContext
    {
        public BamContext() 
        {
        }

        static BamContext _current;
        static object _currentLock = new object();
        public static BamContext Current
        {
            get
            {
                return _currentLock.DoubleCheckLock(ref _current, () => new BamContext());
            }
        }

        public static ProcessModeServiceRegistry GetServiceRegistry()
        {
            return GetServiceRegistry(ProcessMode.Current);
        }

        public static ProcessModeServiceRegistry GetServiceRegistry(ProcessMode processMode)
        {
            return Current.registryConfigurers[processMode.Mode];
        }

        Dictionary<ProcessModes, ProcessModeServiceRegistry> registryConfigurers = new Dictionary<ProcessModes, ProcessModeServiceRegistry>
        {
            { 
                ProcessModes.Dev, new ProcessModeServiceRegistry(ProcessModes.Dev, 
                    CommonConfigure(
                        new ServiceRegistry()
                        // Add dev dependencies here
                            .Include(ServiceRegistry.Default)
                        )
                ) 
            },
            { 
                ProcessModes.Test, new ProcessModeServiceRegistry(ProcessModes.Test, 
                    CommonConfigure(
                        new ServiceRegistry()
                        // Add test dependencies here
                            .Include(ServiceRegistry.Default)
                        )
                )
            },
            { 
                ProcessModes.Prod, new ProcessModeServiceRegistry(ProcessModes.Prod, 
                    CommonConfigure(
                        new ServiceRegistry()
                        // Add prod dependencies here
                            .Include(ServiceRegistry.Default)
                        )
                )
            }
        };

        protected static ServiceRegistry CommonConfigure(ServiceRegistry serviceRegistry)
        {
            return serviceRegistry
                .For<IApplicationNameProvider>().Use<ProcessApplicationNameProvider>();
        }

        public static void Configure(Func<ServiceRegistry, ServiceRegistry> configurer)
        {
            Configure(ProcessMode.Current.Mode, configurer);
        }

        public static void Configure(ProcessModes processMode, Func<ServiceRegistry, ServiceRegistry> configurer)
        {
            ProcessModeServiceRegistry existing = Current.registryConfigurers[processMode];
            if (existing != null)
            {
                ServiceRegistry newRegistry = new ServiceRegistry();
                newRegistry.CopyFrom(existing.ServiceRegistry);
                existing.ServiceRegistry = configurer(newRegistry);
            }
        }
    }
}
