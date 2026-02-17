using Bam.Services;

namespace Bam.DependencyInjection
{
    /// <summary>
    /// Provides a fluent API for registering service implementations in a <see cref="ServiceRegistry"/>
    /// for a given service type I.
    /// </summary>
    /// <typeparam name="I">The service type (typically an interface) being configured.</typeparam>
    public class FluentServiceRegistryContext<I>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentServiceRegistryContext{I}"/> class without a registry.
        /// A new <see cref="ServiceRegistry"/> will be created on first use.
        /// </summary>
        public FluentServiceRegistryContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentServiceRegistryContext{I}"/> class with the specified registry.
        /// </summary>
        /// <param name="registry">The service registry to register implementations into.</param>
        public FluentServiceRegistryContext(ServiceRegistry registry)
        {
            ServiceRegistry = registry;
        }

        /// <summary>
        /// Registers the specified instance as the implementation for type I. Alias for <see cref="Returns(object)"/>.
        /// </summary>
        /// <param name="instance">The instance to use when resolving type I.</param>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry Use(object instance)
        {
            return Returns(instance);
        }

        /// <summary>
        /// Registers the specified instance as a factory-backed implementation for type I.
        /// </summary>
        /// <param name="instance">The instance to return when resolving type I.</param>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry Returns(object instance)
        {
            ServiceRegistry svcRegistry = ServiceRegistry ?? new ServiceRegistry();
            svcRegistry.Set(typeof(I), () => instance);
            return svcRegistry;
        }

        /// <summary>
        /// Registers type T as a transient implementation for type I, constructing a new instance on each resolution. Alias for <see cref="Returns{T}()"/>.
        /// </summary>
        /// <typeparam name="T">The implementation type to construct.</typeparam>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry Use<T>()
        {
            return Returns<T>();
        }

        /// <summary>
        /// Registers type T as the implementation for type I, constructing it with the specified constructor arguments.
        /// </summary>
        /// <typeparam name="T">The implementation type to construct.</typeparam>
        /// <param name="ctorArgs">The constructor arguments to pass when constructing type T.</param>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry Use<T>(params object[] ctorArgs)
        {
            object[] args = ctorArgs;
            ServiceRegistry svcRegistry = ServiceRegistry ?? new ServiceRegistry();
            svcRegistry.Set(typeof(I), ()=> svcRegistry.Construct(typeof(T), args));
            return svcRegistry;
        }
        
        /// <summary>
        /// Registers type T as a transient implementation for type I, constructing a new instance on each resolution.
        /// Same as <see cref="To{T}"/>.
        /// </summary>
        /// <typeparam name="T">The implementation type to construct.</typeparam>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry Returns<T>()
        {
            ServiceRegistry svcRegistry = ServiceRegistry ?? new ServiceRegistry();
            svcRegistry.Set(typeof(I), () => svcRegistry.Construct(typeof(T)));
            return svcRegistry;
        }

        /// <summary>
        /// Registers type T as a singleton implementation for type I. The instance is constructed immediately
        /// and the same instance is returned on every resolution.
        /// </summary>
        /// <typeparam name="T">The implementation type to construct once and reuse.</typeparam>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry UseSingleton<T>()
        {
            ServiceRegistry svcRegistry = ServiceRegistry ?? new ServiceRegistry();
            svcRegistry.Set(typeof(I), svcRegistry.Construct(typeof(T)));
            return svcRegistry;
        }

        /// <summary>
        /// Registers the specified instance as a singleton implementation for type I. The same instance
        /// is returned on every resolution.
        /// </summary>
        /// <typeparam name="T">The implementation type.</typeparam>
        /// <param name="instance">The pre-created instance to register. Must not be null.</param>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        public ServiceRegistry UseSingleton<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            ServiceRegistry svcRegistry = ServiceRegistry ?? new ServiceRegistry();
            svcRegistry.Set(typeof(I), instance);
            return svcRegistry;
        }

        /// <summary>
        /// Registers type T as a transient implementation for type I. A new instance is constructed on each resolution.
        /// Alias for <see cref="Returns{T}()"/>.
        /// </summary>
        /// <typeparam name="T">The implementation type to construct.</typeparam>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry UseTransient<T>()
        {
            return Returns<T>();
        }

        /// <summary>
        /// Registers a factory function as the implementation for type I. Alias for <see cref="Returns{T}(Func{T})"/>.
        /// </summary>
        /// <typeparam name="T">The implementation type produced by the factory.</typeparam>
        /// <param name="instanciator">The factory function invoked on each resolution.</param>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry Use<T>(Func<T> instanciator)
        {
            return Returns<T>(instanciator);
        }

        /// <summary>
        /// Registers a factory function as the implementation for type I.
        /// </summary>
        /// <typeparam name="T">The implementation type produced by the factory.</typeparam>
        /// <param name="instanciator">The factory function invoked on each resolution.</param>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry Returns<T>(Func<T> instanciator)
        {
            ServiceRegistry inc = ServiceRegistry ?? new ServiceRegistry();
            inc.Set(typeof(I), instanciator, false);
            return inc;
        }

        /// <summary>
        /// Registers a factory function that receives the <see cref="ServiceRegistry"/> as the implementation for type I.
        /// Alias for <see cref="Returns{T}(Func{ServiceRegistry, T})"/>.
        /// </summary>
        /// <typeparam name="T">The implementation type produced by the factory.</typeparam>
        /// <param name="instanciator">The factory function that receives the registry and returns an instance of T.</param>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry Use<T>(Func<ServiceRegistry, T> instanciator)
        {
            return Returns<T>(instanciator);
        }

        /// <summary>
        /// Registers a factory function that receives the <see cref="ServiceRegistry"/> as the implementation for type I.
        /// </summary>
        /// <typeparam name="T">The implementation type produced by the factory.</typeparam>
        /// <param name="instanciator">The factory function that receives the registry and returns an instance of T.</param>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry Returns<T>(Func<ServiceRegistry, T> instanciator)
        {
            ServiceRegistry svcRegistry = ServiceRegistry ?? new ServiceRegistry();
            T UseThis() => instanciator(svcRegistry); // local function
            svcRegistry.Set(typeof(I), (Func<T>)UseThis);
            return svcRegistry;
        }

        /// <summary>
        /// Registers type T as a transient implementation for type I. Same as <see cref="Returns{T}()"/>.
        /// </summary>
        /// <typeparam name="T">The implementation type to construct.</typeparam>
        /// <returns>The <see cref="ServiceRegistry"/> containing the registration.</returns>
        public ServiceRegistry To<T>()
        {
            return Returns<T>();
        }

        /// <summary>
        /// Gets or sets the underlying service registry that registrations are applied to.
        /// </summary>
        protected ServiceRegistry ServiceRegistry
        {
            get;
            set;
        } = null!;
    }
}
