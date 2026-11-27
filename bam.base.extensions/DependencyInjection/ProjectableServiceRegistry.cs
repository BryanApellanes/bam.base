using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bam.DependencyInjection;

/// <summary>
/// A <see cref="ServiceRegistry"/> that records what it declares, so the same declaration can be projected into a
/// Microsoft <see cref="IServiceCollection"/> and an ASP.NET Core host runs with one container (bam.base#58, generalised
/// from bamgateway's <c>GatewayServiceRegistry</c>). Declarations come in four kinds:
/// <list type="bullet">
/// <item><description>a <em>seam</em>, one implementation of a service (<see cref="UseSeam{TService, TImplementation}"/>);</description></item>
/// <item><description>a <em>catalog</em>, an ordered set of implementations of one service (<see cref="AddToCatalog{TService, TImplementation}"/>);</description></item>
/// <item><description>an <em>extension</em>, one more registration of a service the host composes as a collection (<see cref="AddExtension(Type, Type)"/>);</description></item>
/// <item><description>a <em>composed</em> binding the host builds itself and nobody may rebind (<see cref="Compose{TService, TImplementation}"/>).</description></item>
/// </list>
/// A plain <see cref="ServiceRegistry"/> can't tell which bindings were declared as replaceable and which arrived some
/// other way (<c>For&lt;I&gt;().Use</c>, <c>Set</c>, <c>Include</c>, <c>CopyFrom</c>), so the projection lives on this type,
/// which can: <see cref="ProjectInto(IServiceCollection, ProjectionOptions?)"/> refuses, naming every conflict, rather
/// than let the host run a different type than <c>registry.Get</c> returns, or silently replace a choice the host made in
/// the collection (a strict policy swapped for a pass-through default fails open).
/// </summary>
public class ProjectableServiceRegistry : ServiceRegistry
{
    private readonly Dictionary<Type, Type> _seams = new Dictionary<Type, Type>();
    private readonly Dictionary<Type, Func<object>> _seamRegistrations = new Dictionary<Type, Func<object>>();
    private readonly Dictionary<Type, List<Type>> _catalogs = new Dictionary<Type, List<Type>>();
    private readonly List<Type> _catalogOrder = new List<Type>();
    private readonly List<ServiceDescriptor> _extensions = new List<ServiceDescriptor>();
    private readonly Dictionary<Type, object> _composedRegistrations = new Dictionary<Type, object>();
    private readonly List<ServiceDescriptor> _composedProjections = new List<ServiceDescriptor>();

    /// <summary>Single-implementation seams (service type to implementation type) declared so far.</summary>
    public IReadOnlyDictionary<Type, Type> Seams => _seams;

    /// <summary>The extensions declared, in declaration order.</summary>
    public IReadOnlyList<ServiceDescriptor> Extensions => _extensions;

    /// <summary>The service types bound with <c>Compose</c>, which the registry refuses to see rebound.</summary>
    public IReadOnlyCollection<Type> ComposedTypes => _composedRegistrations.Keys;

    /// <summary>
    /// Declares the single implementation of a seam. The last declaration wins. The registry keeps the registration it
    /// makes, so <see cref="RegistryConflicts(ProjectionOptions?)"/> can tell when the seam was bound again some other way.
    /// </summary>
    /// <typeparam name="TService">The seam's service type.</typeparam>
    /// <typeparam name="TImplementation">The implementation.</typeparam>
    /// <returns>This registry.</returns>
    public ProjectableServiceRegistry UseSeam<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        Func<object> registration = () => Construct(typeof(TImplementation));
        _seams[typeof(TService)] = typeof(TImplementation);
        if (typeof(TService) == typeof(TImplementation))
        {
            // Construct stores what it builds under the implementation's own type, which is the seam's key here, so the
            // registration can't be compared after a Get; such a seam isn't checked by RegistryConflicts.
            _seamRegistrations.Remove(typeof(TService));
        }
        else
        {
            _seamRegistrations[typeof(TService)] = registration;
        }
        Set(typeof(TService), registration);
        return this;
    }

    /// <summary>
    /// Adds an implementation to the catalog of <typeparamref name="TService"/>. A type already in the catalog is
    /// ignored, so shared registration helpers compose safely. The implementation is bound to itself on the registry,
    /// so <see cref="BuildCatalog{TService}"/> can construct it.
    /// </summary>
    /// <typeparam name="TService">The service the catalog holds.</typeparam>
    /// <typeparam name="TImplementation">The implementation to add.</typeparam>
    /// <returns>This registry.</returns>
    public ProjectableServiceRegistry AddToCatalog<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        List<Type> catalog = Catalog(typeof(TService));
        Type implementation = typeof(TImplementation);
        if (!catalog.Contains(implementation))
        {
            catalog.Add(implementation);
            For<TImplementation>().Use<TImplementation>();
        }
        return this;
    }

    /// <summary>The implementations in the catalog of <typeparamref name="TService"/>, in the order they were added.</summary>
    /// <typeparam name="TService">The service the catalog holds.</typeparam>
    /// <returns>The implementation types; empty when nothing was added.</returns>
    public IReadOnlyList<Type> CatalogTypes<TService>() where TService : class => CatalogTypes(typeof(TService));

    /// <summary>The implementations in the catalog of <paramref name="serviceType"/>, in the order they were added.</summary>
    /// <param name="serviceType">The service the catalog holds.</param>
    /// <returns>The implementation types; empty when nothing was added.</returns>
    public IReadOnlyList<Type> CatalogTypes(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return _catalogs.TryGetValue(serviceType, out List<Type>? catalog) ? catalog : Array.Empty<Type>();
    }

    /// <summary>Constructs one instance of every implementation in the catalog from this registry, in order (for standalone and test use).</summary>
    /// <typeparam name="TService">The service the catalog holds.</typeparam>
    /// <returns>The instances.</returns>
    public IReadOnlyList<TService> BuildCatalog<TService>() where TService : class
    {
        IReadOnlyList<Type> types = CatalogTypes(typeof(TService));
        List<TService> built = new List<TService>(types.Count);
        foreach (Type type in types)
        {
            built.Add((TService)Get(type));
        }
        return built;
    }

    /// <summary>
    /// Declares one more implementation of a service the host composes as a collection, projected as an enumerable
    /// singleton. The same type declared twice is one extension.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">A type implementing it, constructed by the container.</param>
    /// <returns>This registry.</returns>
    /// <exception cref="ArgumentException"><paramref name="implementationType"/> doesn't implement <paramref name="serviceType"/>.</exception>
    public ProjectableServiceRegistry AddExtension(Type serviceType, Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);
        if (!serviceType.IsAssignableFrom(implementationType))
        {
            throw new ArgumentException($"{implementationType.Name} does not implement {serviceType.Name}.", nameof(implementationType));
        }
        _extensions.Add(ServiceDescriptor.Singleton(serviceType, implementationType));
        return this;
    }

    /// <summary>
    /// Declares one more instance of a service the host composes as a collection, projected as a singleton. The same
    /// instance declared twice is one extension, as a repeated type is: a shared helper mustn't register a provider twice.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="instance">The instance.</param>
    /// <returns>This registry.</returns>
    /// <exception cref="ArgumentException"><paramref name="instance"/> doesn't implement <paramref name="serviceType"/>.</exception>
    public ProjectableServiceRegistry AddExtension(Type serviceType, object instance)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(instance);
        if (!serviceType.IsInstanceOfType(instance))
        {
            throw new ArgumentException($"The instance does not implement {serviceType.Name}.", nameof(instance));
        }
        if (_extensions.Any(existing => existing.ServiceType == serviceType && ReferenceEquals(existing.ImplementationInstance, instance)))
        {
            return this;
        }
        _extensions.Add(ServiceDescriptor.Singleton(serviceType, instance));
        return this;
    }

    /// <summary>
    /// Binds a service the host composes itself, constructed as <typeparamref name="TImplementation"/> both on the
    /// registry and in the collection (projected with <c>TryAddSingleton</c>). Rebinding or removing it on the registry
    /// afterwards is a conflict. A service that is its own implementation isn't compared, as with a seam.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <typeparam name="TImplementation">The implementation.</typeparam>
    /// <returns>This registry.</returns>
    public ProjectableServiceRegistry Compose<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        For<TService>().Use<TImplementation>();
        return RecordComposed(typeof(TService), ServiceDescriptor.Singleton(typeof(TService), typeof(TImplementation)));
    }

    /// <summary>
    /// Binds a service the host composes itself with two factories: one for the registry and, optionally, one for the
    /// host's container. Without <paramref name="hostFactory"/>, nothing is projected for it and the host registers it
    /// itself. Rebinding or removing it on the registry afterwards is a conflict.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="registryFactory">Builds the service from this registry.</param>
    /// <param name="hostFactory">Builds the service from the host's provider, projected with <c>TryAddSingleton</c>.</param>
    /// <returns>This registry.</returns>
    public ProjectableServiceRegistry Compose<TService>(Func<ServiceRegistry, TService> registryFactory, Func<IServiceProvider, TService>? hostFactory = null)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(registryFactory);
        For<TService>().Use(registryFactory);
        ServiceDescriptor? projection = hostFactory is null ? null : ServiceDescriptor.Singleton(typeof(TService), hostFactory);
        return RecordComposed(typeof(TService), projection);
    }

    /// <summary>
    /// Registers everything declared here into <paramref name="services"/>: seams with <c>Replace</c> as singletons (the
    /// last declaration wins), catalogs with <c>TryAddEnumerable</c> in order, extensions (instances with <c>Add</c>, types
    /// with <c>TryAddEnumerable</c>), then composed bindings with <c>TryAdd</c>.
    /// </summary>
    /// <remarks>
    /// It first checks <see cref="RegistryConflicts(ProjectionOptions?)"/> and <see cref="SeamConflicts(IServiceCollection, ProjectionOptions?)"/>
    /// and, when either finds anything, throws one <see cref="InvalidOperationException"/> listing them all, and registers
    /// nothing. A registration added to the collection after this call isn't checked: <c>TryAdd</c> then does nothing, and
    /// <c>Add</c> wins over the projected seam.
    /// </remarks>
    /// <param name="services">The host's service collection.</param>
    /// <param name="options">The known defaults and message wording; null for none.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <exception cref="InvalidOperationException">A seam or composed binding conflicts; the message names each one.</exception>
    public IServiceCollection ProjectInto(IServiceCollection services, ProjectionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ProjectionOptions resolved = options ?? new ProjectionOptions();
        IReadOnlyList<string> registryConflicts = RegistryConflicts(resolved);
        IReadOnlyList<string> seamConflicts = SeamConflicts(services, resolved);
        if (registryConflicts.Count > 0 || seamConflicts.Count > 0)
        {
            string name = RegistryLabel(resolved);
            List<string> message = new List<string>();
            if (registryConflicts.Count > 0)
            {
                message.Add($"The {name} registry holds a binding the service collection would not get. " + string.Join(" ", registryConflicts));
            }
            if (seamConflicts.Count > 0)
            {
                string advice = string.IsNullOrWhiteSpace(resolved.SeamConflictAdvice) ? "Or remove the direct registration." : "Or remove the direct registration; " + resolved.SeamConflictAdvice;
                message.Add($"The {name} registry would silently replace a seam the host registered directly in the service collection. " + string.Join(" ", seamConflicts) + " " + advice);
            }
            throw new InvalidOperationException(string.Join(" ", message));
        }
        foreach (KeyValuePair<Type, Type> seam in _seams)
        {
            services.Replace(ServiceDescriptor.Singleton(seam.Key, seam.Value));
        }
        foreach (Type serviceType in _catalogOrder)
        {
            foreach (Type implementation in _catalogs[serviceType])
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(serviceType, implementation));
            }
        }
        foreach (ServiceDescriptor extension in _extensions)
        {
            if (extension.ImplementationInstance is not null)
            {
                services.Add(extension);
            }
            else
            {
                services.TryAddEnumerable(extension);
            }
        }
        foreach (ServiceDescriptor composed in _composedProjections)
        {
            services.TryAdd(composed);
        }
        return services;
    }

    /// <summary>
    /// One sentence for each registration in <paramref name="services"/> that projecting would replace with something
    /// else: a declared seam whose existing registration is neither the declared type nor a known default. A
    /// registration made with a factory or an instance has no type to compare, so it counts. Keyed registrations aren't
    /// seams and are ignored. Lifetimes aren't compared: a seam is projected as a singleton.
    /// </summary>
    /// <param name="services">The host's service collection.</param>
    /// <param name="options">The known defaults and message wording; null for none.</param>
    /// <returns>The conflicts, empty when projecting is safe.</returns>
    public IReadOnlyList<string> SeamConflicts(IServiceCollection services, ProjectionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ProjectionOptions resolved = options ?? new ProjectionOptions();
        string place = string.IsNullOrWhiteSpace(resolved.DeclarationPlace) ? string.Empty : $", {resolved.DeclarationPlace}";
        List<string> conflicts = new List<string>();
        foreach (KeyValuePair<Type, Type> seam in _seams)
        {
            resolved.KnownDefaults.TryGetValue(seam.Key, out Type? knownDefault);
            foreach (ServiceDescriptor existing in services.Where(descriptor => descriptor.ServiceType == seam.Key && !descriptor.IsKeyedService))
            {
                Type? registered = existing.ImplementationType;
                if (registered is not null && (registered == seam.Value || registered == knownDefault))
                {
                    continue;
                }
                string found = registered is not null
                    ? DisplayName(registered)
                    : existing.ImplementationInstance is not null
                        ? $"an instance of {DisplayName(existing.ImplementationInstance.GetType())}"
                        : "a factory";
                conflicts.Add($"{DisplayName(seam.Key)} is registered as {found}, and the registry declares {DisplayName(seam.Value)}. Declare it on the registry instead{place}: {Declaration(seam.Key, resolved)}.");
            }
        }
        return conflicts;
    }

    /// <summary>
    /// One sentence for each binding on this registry that projecting would not carry to the host:
    /// <list type="bullet">
    /// <item><description>a declared seam whose registration is no longer the one <see cref="UseSeam{TService, TImplementation}"/> made (rebound with <c>For&lt;I&gt;().Use</c>, <c>Set</c>, <c>Include</c> or <c>CopyFrom</c>, or removed);</description></item>
    /// <item><description>a seam named in <see cref="ProjectionOptions.DeclarationHints"/> that is bound without a declaration;</description></item>
    /// <item><description>a composed binding rebound or removed.</description></item>
    /// </list>
    /// Nothing is constructed to find out. A seam whose service type is its own implementation isn't checked.
    /// </summary>
    /// <param name="options">The hints and message wording; null for none.</param>
    /// <returns>The conflicts, empty when every binding is the one its declaration made.</returns>
    public IReadOnlyList<string> RegistryConflicts(ProjectionOptions? options = null)
    {
        ProjectionOptions resolved = options ?? new ProjectionOptions();
        List<string> conflicts = new List<string>();
        foreach (KeyValuePair<Type, object> composed in _composedRegistrations)
        {
            TryGetRegistration(composed.Key, out object? registration);
            if (!ReferenceEquals(registration, composed.Value))
            {
                string found = registration is null ? "removed from" : $"bound again as {DescribeRegistration(registration)} on";
                string advice = resolved.ComposedAdvice.TryGetValue(composed.Key, out string? text) ? " " + text : string.Empty;
                conflicts.Add($"{DisplayName(composed.Key)} was {found} the registry, but the host composes it and cannot be given another.{advice}");
            }
        }
        foreach (Type seam in resolved.DeclarationHints.Keys.Union(_seams.Keys))
        {
            if (_composedRegistrations.ContainsKey(seam))
            {
                continue;
            }
            bool bound = TryGetRegistration(seam, out object? registration);
            if (_seamRegistrations.TryGetValue(seam, out Func<object>? declared))
            {
                if (ReferenceEquals(registration, declared))
                {
                    continue;
                }
                conflicts.Add(registration is null
                    ? $"{DisplayName(seam)} was declared as {DisplayName(_seams[seam])} and then removed from the registry, so the host would run {DisplayName(_seams[seam])} while the registry has no binding for it. Declare it only with {Declaration(seam, resolved)}."
                    : $"{DisplayName(seam)} was declared as {DisplayName(_seams[seam])} and then bound again as {DescribeRegistration(registration)} on the registry, so the host would run {DisplayName(_seams[seam])} while registry.Get returns something else. Declare it only with {Declaration(seam, resolved)}.");
            }
            else if (_seams.ContainsKey(seam))
            {
                // A UseSeam<Foo, Foo>() declaration: not compared, see the summary.
                continue;
            }
            else if (bound && registration is not null)
            {
                conflicts.Add($"{DisplayName(seam)} is bound on the registry as {DescribeRegistration(registration)} without a declaration, so the host would not run it. Declare it with {Declaration(seam, resolved)}.");
            }
        }
        return conflicts;
    }

    /// <summary>
    /// A type's full name, with generic arguments written as C# does (<c>Ns.IFoo&lt;Ns.Bar&gt;</c>, not <c>IFoo`1</c>). A type
    /// nested in a generic type prints its outer type's arguments on itself, as reflection reports them; it's only a message.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The name.</returns>
    public static string DisplayName(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return Format(type, true);
    }

    private ProjectableServiceRegistry RecordComposed(Type serviceType, ServiceDescriptor? projection)
    {
        TryGetRegistration(serviceType, out object? registration);
        _composedRegistrations[serviceType] = registration!;
        _composedProjections.RemoveAll(descriptor => descriptor.ServiceType == serviceType);
        if (projection is not null)
        {
            _composedProjections.Add(projection);
        }
        return this;
    }

    private List<Type> Catalog(Type serviceType)
    {
        if (!_catalogs.TryGetValue(serviceType, out List<Type>? catalog))
        {
            catalog = new List<Type>();
            _catalogs[serviceType] = catalog;
            _catalogOrder.Add(serviceType);
        }
        return catalog;
    }

    private string RegistryLabel(ProjectionOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.RegistryName))
        {
            return options.RegistryName;
        }
        return string.IsNullOrWhiteSpace(Name) ? "service" : Name;
    }

    private static string DescribeRegistration(object registration) => registration is Delegate
        ? "a factory (For<I>().Use..., Set with a factory, or Include/CopyFrom of another registry)"
        : $"an instance of {DisplayName(registration.GetType())}";

    private static string Declaration(Type seam, ProjectionOptions options) => options.DeclarationHints.TryGetValue(seam, out string? hint)
        ? hint
        : $"registry.{nameof(UseSeam)}<{Format(seam, false)}, T>()";

    private static string Format(Type type, bool qualified)
    {
        if (!type.IsGenericType)
        {
            return qualified ? type.FullName ?? type.Name : type.Name;
        }
        string name = type.Name;
        int tick = name.IndexOf('`');
        string stem = tick < 0 ? name : name.Substring(0, tick);
        if (qualified)
        {
            stem = type.DeclaringType is not null
                ? $"{Format(type.DeclaringType, true)}+{stem}"
                : string.IsNullOrEmpty(type.Namespace) ? stem : $"{type.Namespace}.{stem}";
        }
        string arguments = type.IsGenericTypeDefinition
            ? new string(',', type.GetGenericArguments().Length - 1)
            : string.Join(", ", type.GetGenericArguments().Select(argument => Format(argument, qualified)));
        return $"{stem}<{arguments}>";
    }
}
