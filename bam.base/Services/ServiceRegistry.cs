using System.Reflection;

namespace Bam.Services
{
    public class ServiceRegistry: DependencyProvider
    {
        static ServiceRegistry()
        {
            Default = new ServiceRegistry { Name = "Default" };
        }

        public string? Name { get; set; }

        public FluentCtorContext<I> ForCtor<I>(string parameterName)
        {
            return new FluentCtorContext<I>(this, parameterName);
        }

        public FluentServiceRegistryContext<I> For<I>()
        {
            return new FluentServiceRegistryContext<I>(this);
        }
        
        public ServiceRegistry Include(DependencyProvider dependencyProvider)
        {
            CombineWith(dependencyProvider, true);
            return this;
        }

        /// <summary>
        /// Include the configuration from the specified registry into the current, overwriting existing values in the current registry.
        /// </summary>
        /// <param name="registry">The registry to include.</param>
        /// <returns>ServiceRegistry.</returns>
        public ServiceRegistry Include(ServiceRegistry registry)
        {
            CombineWith(registry, true);
            return this;
        }

        public static ServiceRegistry Create()
        {
            return new ServiceRegistry();
        }

        public void Validate()
        {
            ValidateClassNames();
            ValidateClassTypes();
        }

        public void ValidateClassNames()
        {
            foreach (string className in ClassNames)
            {
                object instance = Get(className);
                Expect.IsNotNull(instance, $"{className} was null");
            }
        }

        public void ValidateClassTypes()
        {
            foreach (Type type in ClassNameTypes)
            {
                object instance = this[type];
                Expect.IsNotNull(instance, $"{type.Name} returned null");
            }
        }

        public new static ServiceRegistry? Default { get; set; }

        public static Func<ServiceRegistry> GetServiceLoader(Type type, ServiceRegistry? orDefault = null)
        {
            return GetServiceLoader(type, type.Assembly, orDefault);
        }

        /// <summary>
        /// Gets a function that returns a `ServiceRegistry` instance.  The function returned
        /// is a reference to the `Get` method of the first class found addorned with the
        /// `ServiceRegistryContainer` attribute or the first method of said class addorned
        /// with a `ServiceRegistryLoader` attribute.
        /// </summary>
        /// <param name="type">The type whose assembly is searched.</param>
        /// <param name="orDefault"></param>
        /// <returns></returns>
        public static Func<ServiceRegistry> GetServiceLoader(Type type, Assembly assembly, ServiceRegistry? orDefault = null)
        {
            if (Default == null)
            {
                Type? coreRegistryContainer = assembly.GetTypes().FirstOrDefault(t => t.HasCustomAttributeOfType<ServiceRegistryContainerAttribute>());
                if (coreRegistryContainer != null)
                {
                    MethodInfo? provider = coreRegistryContainer.GetMethods().FirstOrDefault(mi => 
                        (mi.HasCustomAttributeOfType(out ServiceRegistryLoaderAttribute attr) &&
                        attr.ProcessModes.Contains(ProcessMode.Current.Mode)) || mi.Name.Equals("Get"));
                    
                    if (provider != null)
                    {
                        object instance = provider.IsStatic ? null : provider.DeclaringType.Construct();
                        Default = (ServiceRegistry)provider.Invoke(instance, null);
                    }
                }
            }
            return Default == null ? (() => type.Construct<ServiceRegistry>()) : () =>
            {
                if (!Default.TryGet(out ServiceRegistry result))
                {
                    result = orDefault;
                }
                return result;
            };
        }
    }
}
