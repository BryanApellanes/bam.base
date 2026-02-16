using System.Reflection;
using Bam.Services;

namespace Bam.DependencyInjection
{
    /// <summary>
    /// A named dependency injection registry that extends <see cref="DependencyProvider"/> with fluent registration APIs
    /// and validation capabilities.
    /// </summary>
    public class ServiceRegistry: DependencyProvider
    {
        static ServiceRegistry()
        {
            Default = new ServiceRegistry { Name = "Default" };
        }

        /// <summary>
        /// Gets or sets the name of this service registry.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Begins a fluent constructor parameter registration for type I.
        /// </summary>
        /// <typeparam name="I">The type whose constructor parameter is being configured.</typeparam>
        /// <param name="parameterName">The name of the constructor parameter to configure.</param>
        /// <returns>A <see cref="FluentCtorContext{I}"/> for specifying the parameter value.</returns>
        public FluentCtorContext<I> ForCtor<I>(string parameterName)
        {
            return new FluentCtorContext<I>(this, parameterName);
        }

        /// <summary>
        /// Begins a fluent service registration for type I, allowing specification of the implementation type or factory.
        /// </summary>
        /// <typeparam name="I">The service type (typically an interface) to register an implementation for.</typeparam>
        /// <returns>A <see cref="FluentServiceRegistryContext{I}"/> for specifying the implementation.</returns>
        public FluentServiceRegistryContext<I> For<I>()
        {
            return new FluentServiceRegistryContext<I>(this);
        }

        /// <summary>
        /// Merges all registrations from the specified dependency provider into this registry, overwriting existing values.
        /// </summary>
        /// <param name="dependencyProvider">The dependency provider whose registrations should be included.</param>
        /// <returns>This <see cref="ServiceRegistry"/> instance for chaining.</returns>
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

        /// <summary>
        /// Creates a new empty <see cref="ServiceRegistry"/> instance.
        /// </summary>
        /// <returns>A new <see cref="ServiceRegistry"/>.</returns>
        public static ServiceRegistry Create()
        {
            return new ServiceRegistry();
        }

        /// <summary>
        /// Validates that all registered class names and class types can be resolved to non-null instances.
        /// Throws an <see cref="ExpectationFailedException"/> if any resolution returns null.
        /// </summary>
        public void Validate()
        {
            ValidateClassNames();
            ValidateClassTypes();
        }

        /// <summary>
        /// Validates that every registered class name resolves to a non-null instance.
        /// </summary>
        public void ValidateClassNames()
        {
            foreach (string className in ClassNames)
            {
                object instance = Get(className);
                Expect.IsNotNull(instance, $"{className} was null");
            }
        }

        /// <summary>
        /// Validates that every registered class type resolves to a non-null instance.
        /// </summary>
        public void ValidateClassTypes()
        {
            foreach (Type type in ClassNameTypes)
            {
                object instance = this[type];
                Expect.IsNotNull(instance, $"{type.Name} returned null");
            }
        }

        /// <summary>
        /// Gets or sets the default global <see cref="ServiceRegistry"/> instance. Hides the base <see cref="DependencyProvider.Default"/>.
        /// </summary>
        public new static ServiceRegistry? Default { get; set; }

        /// <summary>
        /// Gets a function that returns a <see cref="ServiceRegistry"/> by searching the specified type's assembly
        /// for a class adorned with <see cref="ServiceRegistryContainerAttribute"/>.
        /// </summary>
        /// <param name="type">The type whose assembly is searched for a service registry container.</param>
        /// <param name="orDefault">An optional fallback registry to use if no registry is found.</param>
        /// <returns>A function that, when invoked, returns the resolved <see cref="ServiceRegistry"/>.</returns>
        public static Func<ServiceRegistry> GetServiceLoader(Type type, ServiceRegistry? orDefault = null)
        {
            return GetServiceLoader(type, type.Assembly, orDefault);
        }

        /// <summary>
        /// Gets a function that returns a <see cref="ServiceRegistry"/> instance. The function returned
        /// is a reference to the <c>Get</c> method of the first class found adorned with the
        /// <see cref="ServiceRegistryContainerAttribute"/> or the first method of said class adorned
        /// with a <see cref="ServiceRegistryLoaderAttribute"/>.
        /// </summary>
        /// <param name="type">The type used as a fallback to construct a <see cref="ServiceRegistry"/> if none is found.</param>
        /// <param name="assembly">The assembly to search for a service registry container.</param>
        /// <param name="orDefault">An optional fallback registry to use if no registry is found in the container.</param>
        /// <returns>A function that, when invoked, returns the resolved <see cref="ServiceRegistry"/>.</returns>
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
