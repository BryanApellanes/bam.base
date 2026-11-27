namespace Bam.DependencyInjection;

/// <summary>
/// How a <see cref="ProjectableServiceRegistry"/> judges and words conflicts when it projects into a host's service
/// collection (bam.base#58).
/// </summary>
public sealed class ProjectionOptions
{
    /// <summary>
    /// Gets the name the conflict messages give the registry, as in "The {RegistryName} registry ...". When empty, the
    /// registry's own <see cref="ServiceRegistry.Name"/> is used.
    /// </summary>
    public string? RegistryName { get; init; }

    /// <summary>
    /// Gets the defaults the host's own registration code adds to the collection for seams the registry also declares
    /// (service type to implementation type). Finding one of these in the collection is expected, so it is not a conflict.
    /// </summary>
    public IReadOnlyDictionary<Type, Type> KnownDefaults { get; init; } = new Dictionary<Type, Type>();

    /// <summary>
    /// Gets the call a host should make to declare each named seam (service type to text such as
    /// <c>registry.UseRouteResolver&lt;T&gt;()</c>), used in the messages. A seam named here is checked by
    /// <see cref="ProjectableServiceRegistry.RegistryConflicts(ProjectionOptions?)"/> even when it was never declared,
    /// so binding it with <c>For&lt;I&gt;().Use</c> alone is reported. A seam with no hint is described as
    /// <c>registry.UseSeam&lt;IService, T&gt;()</c>.
    /// </summary>
    public IReadOnlyDictionary<Type, string> DeclarationHints { get; init; } = new Dictionary<Type, string>();

    /// <summary>
    /// Gets where the host declares seams, added to the seam-conflict advice (for example "in the AddBamGateway
    /// callback"). Optional.
    /// </summary>
    public string? DeclarationPlace { get; init; }

    /// <summary>
    /// Gets a sentence appended to the seam-conflict message (for example, where in start-up a registration that adds a
    /// default belongs). Optional.
    /// </summary>
    public string? SeamConflictAdvice { get; init; }

    /// <summary>
    /// Gets the advice the message gives for a composed binding that was rebound or removed (for example "Add policies
    /// with registry.AddPolicy&lt;T&gt;() instead."). Optional.
    /// </summary>
    public IReadOnlyDictionary<Type, string> ComposedAdvice { get; init; } = new Dictionary<Type, string>();
}
