using Bam.Configuration;
using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Services;

namespace Bam
{
    public abstract class BamContext : IBamContext
    {
        public BamContext() 
        {
        }

        ServiceRegistry _serviceRegistry;
        readonly object _serviceRegistryLock = new object();
        public virtual ServiceRegistry ServiceRegistry
        {
            get => _serviceRegistryLock.DoubleCheckLock(ref _serviceRegistry, GetDefaultContextServiceRegistry);
            set => _serviceRegistry = value;
        }

        public virtual ServiceRegistry GetDefaultContextServiceRegistry()
        {
            return GetServiceRegistry();
        }

        static BamContext? _current;
        static readonly object _currentLock = new object();
        public static BamContext? Current
        {
            get
            {
                return _currentLock.DoubleCheckLock(ref _current, () => new DefaultBamContext());
            }
        }

        public virtual IApplicationNameProvider ApplicationNameProvider => ServiceRegistry.Get<IApplicationNameProvider>();

        public virtual IConfigurationProvider ConfigurationProvider => ServiceRegistry.Get<IConfigurationProvider>();

        public virtual ILogger Logger => ServiceRegistry.Get<ILogger>();

        public static ProcessModeServiceRegistry? GetServiceRegistry()
        {
            return GetServiceRegistry(ProcessMode.Current);
        }

        public static ProcessModeServiceRegistry? GetServiceRegistry(ProcessMode processMode)
        {
            return Current?.registryConfigurers[processMode.Mode];
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
