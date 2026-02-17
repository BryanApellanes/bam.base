using Bam.Configuration;
using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Services;

namespace Bam
{
    /// <summary>
    /// Abstract base class providing environment context for a BAM application, including
    /// service registry management, configuration, and logging scoped by process mode (Dev, Test, Prod).
    /// </summary>
    public abstract class BamContext : IBamContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BamContext"/> class.
        /// </summary>
        public BamContext()
        {
        }

        ServiceRegistry _serviceRegistry = null!;
        readonly object _serviceRegistryLock = new object();
        /// <summary>
        /// Gets or sets the service registry for this context, lazily initialized via <see cref="GetDefaultContextServiceRegistry"/>.
        /// </summary>
        public virtual ServiceRegistry ServiceRegistry
        {
            get => _serviceRegistryLock.DoubleCheckLock(ref _serviceRegistry, GetDefaultContextServiceRegistry);
            set => _serviceRegistry = value;
        }

        /// <summary>
        /// Gets the default service registry for this context by resolving the registry for the current process mode.
        /// </summary>
        /// <returns>The default <see cref="ServiceRegistry"/> for this context.</returns>
        public virtual ServiceRegistry GetDefaultContextServiceRegistry()
        {
            return GetServiceRegistry()!;
        }

        static BamContext? _current;
        static readonly object _currentLock = new object();
        /// <summary>
        /// Gets the current <see cref="BamContext"/> instance, lazily initialized to a <see cref="DefaultBamContext"/>.
        /// </summary>
        public static BamContext? Current
        {
            get
            {
                return _currentLock.DoubleCheckLock(ref _current, () => new DefaultBamContext());
            }
        }

        /// <summary>
        /// Gets the application name provider resolved from the service registry.
        /// </summary>
        public virtual IApplicationNameProvider ApplicationNameProvider => ServiceRegistry.Get<IApplicationNameProvider>();

        /// <summary>
        /// Gets the configuration provider resolved from the service registry.
        /// </summary>
        public virtual IConfigurationProvider ConfigurationProvider => ServiceRegistry.Get<IConfigurationProvider>();

        /// <summary>
        /// Gets the logger resolved from the service registry.
        /// </summary>
        public virtual ILogger Logger => ServiceRegistry.Get<ILogger>();

        /// <summary>
        /// Gets the <see cref="ProcessModeServiceRegistry"/> for the current process mode.
        /// </summary>
        /// <returns>The service registry for the current process mode, or null if not configured.</returns>
        public static ProcessModeServiceRegistry? GetServiceRegistry()
        {
            return GetServiceRegistry(ProcessMode.Current);
        }

        /// <summary>
        /// Gets the <see cref="ProcessModeServiceRegistry"/> for the specified process mode.
        /// </summary>
        /// <param name="processMode">The process mode to get the service registry for.</param>
        /// <returns>The service registry for the specified process mode, or null if not configured.</returns>
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
                            .Include(ServiceRegistry.Default!)
                        )
                ) 
            },
            { 
                ProcessModes.Test, new ProcessModeServiceRegistry(ProcessModes.Test, 
                    CommonConfigure(
                        new ServiceRegistry()
                        // Add test dependencies here
                            .Include(ServiceRegistry.Default!)
                        )
                )
            },
            { 
                ProcessModes.Prod, new ProcessModeServiceRegistry(ProcessModes.Prod, 
                    CommonConfigure(
                        new ServiceRegistry()
                        // Add prod dependencies here
                            .Include(ServiceRegistry.Default!)
                        )
                )
            }
        };

        /// <summary>
        /// Applies common service registrations shared across all process modes.
        /// </summary>
        /// <param name="serviceRegistry">The service registry to configure.</param>
        /// <returns>The configured <see cref="ServiceRegistry"/>.</returns>
        protected static ServiceRegistry CommonConfigure(ServiceRegistry serviceRegistry)
        {
            return serviceRegistry
                .For<IApplicationNameProvider>().Use<ProcessApplicationNameProvider>();
        }

        /// <summary>
        /// Configures the service registry for the current process mode using the specified configuration function.
        /// </summary>
        /// <param name="configurer">A function that receives a copy of the current registry and returns a configured registry.</param>
        public static void Configure(Func<ServiceRegistry, ServiceRegistry> configurer)
        {
            Configure(ProcessMode.Current.Mode, configurer);
        }

        /// <summary>
        /// Configures the service registry for the specified process mode using the specified configuration function.
        /// </summary>
        /// <param name="processMode">The process mode whose service registry should be configured.</param>
        /// <param name="configurer">A function that receives a copy of the current registry and returns a configured registry.</param>
        public static void Configure(ProcessModes processMode, Func<ServiceRegistry, ServiceRegistry> configurer)
        {
            ProcessModeServiceRegistry existing = Current!.registryConfigurers[processMode];
            if (existing != null)
            {
                ServiceRegistry newRegistry = new ServiceRegistry();
                newRegistry.CopyFrom(existing.ServiceRegistry);
                existing.ServiceRegistry = configurer(newRegistry);
            }
        }
    }
}
