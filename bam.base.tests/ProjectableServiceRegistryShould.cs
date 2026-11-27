using Bam.DependencyInjection;
using Bam.Test;
using Microsoft.Extensions.DependencyInjection;

namespace Bam.Tests
{
    /// <summary>Unit coverage for <see cref="ProjectableServiceRegistry"/> (bam.base#58).</summary>
    [UnitTestMenu("Projectable service registry should", Selector = "psr")]
    public class ProjectableServiceRegistryShould
    {
        [UnitTest]
        public void ProjectTheLastSeamDeclared()
        {
            When.A<ProjectableServiceRegistry>("declares a seam twice and projects",
                () => new ProjectableServiceRegistry().UseSeam<IGreeter, PoliteGreeter>().UseSeam<IGreeter, CurtGreeter>(),
                registry =>
                {
                    ServiceCollection services = new ServiceCollection();
                    registry.ProjectInto(services);
                    using ServiceProvider provider = services.BuildServiceProvider();
                    return new SeamOutcome(
                        services.Count(descriptor => descriptor.ServiceType == typeof(IGreeter)),
                        provider.GetRequiredService<IGreeter>().GetType(),
                        registry.Get<IGreeter>().GetType());
                })
                .TheTest
                .ShouldPass<SeamOutcome>((because, outcome) =>
                {
                    because.ItsTrue("the collection holds one registration for the seam", outcome.Registrations == 1);
                    because.ItsTrue("the host resolves the last declaration", outcome.HostType == typeof(CurtGreeter));
                    because.ItsTrue("and so does the registry", outcome.RegistryType == typeof(CurtGreeter));
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void ProjectACatalogInOrderWithoutDuplicates()
        {
            When.A<ProjectableServiceRegistry>("adds StepB, StepA, StepB to a catalog and projects",
                () => new ProjectableServiceRegistry().AddToCatalog<IStep, StepB>().AddToCatalog<IStep, StepA>().AddToCatalog<IStep, StepB>(),
                registry =>
                {
                    ServiceCollection services = new ServiceCollection();
                    registry.ProjectInto(services);
                    using ServiceProvider provider = services.BuildServiceProvider();
                    return new CatalogOutcome(
                        registry.CatalogTypes<IStep>().ToList(),
                        provider.GetServices<IStep>().Select(step => step.GetType()).ToList(),
                        registry.BuildCatalog<IStep>().Select(step => step.GetType()).ToList(),
                        registry.CatalogTypes<IGreeter>().Count);
                })
                .TheTest
                .ShouldPass<CatalogOutcome>((because, outcome) =>
                {
                    because.ItsTrue("the catalog keeps the first order, once each", outcome.Declared.SequenceEqual(new[] { typeof(StepB), typeof(StepA) }));
                    because.ItsTrue("the host resolves the same order", outcome.Hosted.SequenceEqual(new[] { typeof(StepB), typeof(StepA) }));
                    because.ItsTrue("the registry builds the same order", outcome.Built.SequenceEqual(new[] { typeof(StepB), typeof(StepA) }));
                    because.ItsTrue("an empty catalog reads as empty", outcome.EmptyCount == 0);
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void ProjectExtensionsByTypeAndByInstanceOnce()
        {
            StepA shared = new StepA();
            When.A<ProjectableServiceRegistry>("declares extensions by type twice and by the same instance twice",
                () => new ProjectableServiceRegistry()
                    .AddExtension(typeof(IStep), typeof(StepB))
                    .AddExtension(typeof(IStep), typeof(StepB))
                    .AddExtension(typeof(IStep), shared)
                    .AddExtension(typeof(IStep), shared),
                registry =>
                {
                    ServiceCollection services = new ServiceCollection();
                    registry.ProjectInto(services);
                    using ServiceProvider provider = services.BuildServiceProvider();
                    List<IStep> steps = provider.GetServices<IStep>().ToList();
                    string refused = string.Empty;
                    try
                    {
                        registry.AddExtension(typeof(IStep), typeof(PoliteGreeter));
                    }
                    catch (ArgumentException exception)
                    {
                        refused = exception.Message;
                    }
                    return new ExtensionOutcome(steps.Count, steps.Any(step => ReferenceEquals(step, shared)), steps.Count(step => step is StepB), refused);
                })
                .TheTest
                .ShouldPass<ExtensionOutcome>((because, outcome) =>
                {
                    because.ItsTrue($"the host gets two extensions ({outcome.Count})", outcome.Count == 2);
                    because.ItsTrue("one is the declared instance", outcome.HasInstance);
                    because.ItsTrue("and the type is registered once", outcome.TypeCount == 1);
                    because.ItsTrue("a type that doesn't implement the service is refused", outcome.Refused.Contains("does not implement", StringComparison.Ordinal));
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void RefuseASeamTheHostRegisteredDirectlyInEveryForm()
        {
            When.A<ProjectionOptions>("projects onto a collection holding the seam as a type, a factory and an instance",
                () => new ProjectionOptions { RegistryName = "Sample", DeclarationPlace = "in the AddSample callback", SeamConflictAdvice = "AddSample belongs first." },
                options =>
                {
                    ServiceCollection services = new ServiceCollection();
                    services.AddSingleton<IGreeter, CurtGreeter>();
                    services.AddSingleton<IGreeter>(provider => new CurtGreeter());
                    services.AddSingleton<IGreeter>(new CurtGreeter());
                    ProjectableServiceRegistry registry = new ProjectableServiceRegistry().UseSeam<IGreeter, PoliteGreeter>();
                    int before = services.Count;
                    string message = Refusal(() => registry.ProjectInto(services, options));
                    return new RefusalOutcome(message, registry.SeamConflicts(services, options).Count, services.Count == before);
                })
                .TheTest
                .ShouldPass<RefusalOutcome>((because, outcome) =>
                {
                    because.ItsTrue("all three registrations are conflicts", outcome.ConflictCount == 3);
                    because.ItsTrue("the message names the registry", outcome.Message.Contains("The Sample registry would silently replace", StringComparison.Ordinal));
                    because.ItsTrue("it names the type found", outcome.Message.Contains($"is registered as {typeof(CurtGreeter).FullName}", StringComparison.Ordinal));
                    because.ItsTrue("it names the factory", outcome.Message.Contains("is registered as a factory", StringComparison.Ordinal));
                    because.ItsTrue("it names the instance", outcome.Message.Contains($"an instance of {typeof(CurtGreeter).FullName}", StringComparison.Ordinal));
                    because.ItsTrue("it says where and how to declare it", outcome.Message.Contains("instead, in the AddSample callback: registry.UseSeam<IGreeter, T>()", StringComparison.Ordinal));
                    because.ItsTrue("and carries the advice", outcome.Message.EndsWith("AddSample belongs first.", StringComparison.Ordinal));
                    because.ItsTrue("nothing was registered", outcome.Untouched);
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void AcceptAKnownDefaultTheDeclaredTypeAndKeyedRegistrations()
        {
            When.A<ProjectionOptions>("projects onto a collection holding the known default, the declared type and a keyed registration",
                () => new ProjectionOptions { KnownDefaults = new Dictionary<Type, Type> { [typeof(IGreeter)] = typeof(DefaultGreeter) } },
                options =>
                {
                    ServiceCollection services = new ServiceCollection();
                    services.AddSingleton<IGreeter, DefaultGreeter>();
                    services.AddSingleton<IGreeter, PoliteGreeter>();
                    services.AddKeyedSingleton<IGreeter, CurtGreeter>("other");
                    ProjectableServiceRegistry registry = new ProjectableServiceRegistry().UseSeam<IGreeter, PoliteGreeter>();
                    string message = Refusal(() => registry.ProjectInto(services, options));
                    using ServiceProvider provider = services.BuildServiceProvider();
                    return new AcceptOutcome(message, provider.GetRequiredService<IGreeter>().GetType(), provider.GetRequiredKeyedService<IGreeter>("other").GetType());
                })
                .TheTest
                .ShouldPass<AcceptOutcome>((because, outcome) =>
                {
                    because.ItsTrue($"nothing is refused ({outcome.Message})", outcome.Message.Length == 0);
                    because.ItsTrue("the host resolves the declared seam", outcome.HostType == typeof(PoliteGreeter));
                    because.ItsTrue("the keyed registration is left alone", outcome.KeyedType == typeof(CurtGreeter));
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void RefuseASeamReboundOrRemovedOnTheRegistry()
        {
            When.A<ProjectionOptions>("rebinds one declared seam, removes another and binds a hinted one without a declaration",
                () => new ProjectionOptions { DeclarationHints = new Dictionary<Type, string> { [typeof(IStep)] = "registry.UseStep<T>()" } },
                options =>
                {
                    ProjectableServiceRegistry rebound = new ProjectableServiceRegistry().UseSeam<IGreeter, PoliteGreeter>();
                    rebound.For<IGreeter>().Use<CurtGreeter>();
                    ProjectableServiceRegistry removed = new ProjectableServiceRegistry().UseSeam<IGreeter, PoliteGreeter>();
                    removed.Remove(typeof(IGreeter));
                    ProjectableServiceRegistry undeclared = new ProjectableServiceRegistry();
                    undeclared.Set(typeof(IStep), (object)new StepA());
                    ProjectableServiceRegistry fine = new ProjectableServiceRegistry().UseSeam<IGreeter, PoliteGreeter>();
                    fine.Get<IGreeter>();
                    ProjectableServiceRegistry self = new ProjectableServiceRegistry().UseSeam<StepA, StepA>();
                    self.Get<StepA>();
                    return new RegistryOutcome(
                        Refusal(() => rebound.ProjectInto(new ServiceCollection(), options)),
                        Refusal(() => removed.ProjectInto(new ServiceCollection(), options)),
                        Refusal(() => undeclared.ProjectInto(new ServiceCollection(), options)),
                        fine.RegistryConflicts(options).Count + self.RegistryConflicts(options).Count);
                })
                .TheTest
                .ShouldPass<RegistryOutcome>((because, outcome) =>
                {
                    because.ItsTrue("a rebind is refused as a factory", outcome.Rebound.Contains("then bound again as a factory", StringComparison.Ordinal));
                    because.ItsTrue("with the generic declaration", outcome.Rebound.Contains("Declare it only with registry.UseSeam<IGreeter, T>()", StringComparison.Ordinal));
                    because.ItsTrue("a removal is refused", outcome.Removed.Contains("then removed from the registry", StringComparison.Ordinal));
                    because.ItsTrue("a hinted seam bound without a declaration is refused with its hint", outcome.Undeclared.Contains($"as an instance of {typeof(StepA).FullName} without a declaration", StringComparison.Ordinal) && outcome.Undeclared.Contains("registry.UseStep<T>()", StringComparison.Ordinal));
                    because.ItsTrue("resolving a declared seam, or a self seam, is no conflict", outcome.CleanCount == 0);
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void RefuseAComposedBindingReboundOrRemovedAndProjectIt()
        {
            When.A<ProjectionOptions>("composes a pipeline, rebinds it on one registry and removes it on another",
                () => new ProjectionOptions { ComposedAdvice = new Dictionary<Type, string> { [typeof(IPipeline)] = "Add steps instead." } },
                options =>
                {
                    ProjectableServiceRegistry fine = new ProjectableServiceRegistry().Compose<IPipeline, Pipeline>().AddToCatalog<IStep, StepA>();
                    fine.Get<IPipeline>();
                    ServiceCollection services = new ServiceCollection();
                    fine.ProjectInto(services, options);
                    using ServiceProvider provider = services.BuildServiceProvider();
                    ProjectableServiceRegistry factory = new ProjectableServiceRegistry().Compose<IPipeline>(registry => new Pipeline(), provider => new Pipeline());
                    ServiceCollection factoryServices = new ServiceCollection();
                    factory.ProjectInto(factoryServices, options);
                    ProjectableServiceRegistry rebound = new ProjectableServiceRegistry().Compose<IPipeline, Pipeline>();
                    rebound.For<IPipeline>().Use(new Pipeline());
                    ProjectableServiceRegistry removed = new ProjectableServiceRegistry().Compose<IPipeline>(registry => new Pipeline());
                    removed.Remove(typeof(IPipeline));
                    return new ComposedOutcome(
                        provider.GetRequiredService<IPipeline>().GetType(),
                        factoryServices.Count(descriptor => descriptor.ServiceType == typeof(IPipeline) && descriptor.ImplementationFactory is not null),
                        Refusal(() => rebound.ProjectInto(new ServiceCollection(), options)),
                        Refusal(() => removed.ProjectInto(new ServiceCollection(), options)));
                })
                .TheTest
                .ShouldPass<ComposedOutcome>((because, outcome) =>
                {
                    because.ItsTrue("a type-composed binding reaches the host", outcome.HostType == typeof(Pipeline));
                    because.ItsTrue("a factory-composed binding is projected with the host factory", outcome.FactoryRegistrations == 1);
                    because.ItsTrue("a rebind is refused with the advice", outcome.Rebound.Contains("bound again as a factory", StringComparison.Ordinal) && outcome.Rebound.Contains("Add steps instead.", StringComparison.Ordinal));
                    because.ItsTrue("a removal is refused", outcome.Removed.Contains("was removed from the registry", StringComparison.Ordinal));
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void WriteGenericTypeNamesAsCSharpDoes()
        {
            When.A<Type>("formats a closed and an open generic type",
                () => typeof(IHolder<StepA>),
                closed => new NameOutcome(ProjectableServiceRegistry.DisplayName(closed), ProjectableServiceRegistry.DisplayName(typeof(Dictionary<,>))))
                .TheTest
                .ShouldPass<NameOutcome>((because, outcome) =>
                {
                    because.ItsTrue($"a closed type names its arguments ({outcome.Closed})", outcome.Closed == $"Bam.Tests.IHolder<{typeof(StepA).FullName}>");
                    because.ItsTrue($"an open type shows its arity ({outcome.Open})", outcome.Open == "System.Collections.Generic.Dictionary<,>");
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        private static string Refusal(Action project)
        {
            try
            {
                project();
                return string.Empty;
            }
            catch (InvalidOperationException exception)
            {
                return exception.Message;
            }
        }

        private sealed record SeamOutcome(int Registrations, Type HostType, Type RegistryType);

        private sealed record CatalogOutcome(List<Type> Declared, List<Type> Hosted, List<Type> Built, int EmptyCount);

        private sealed record ExtensionOutcome(int Count, bool HasInstance, int TypeCount, string Refused);

        private sealed record RefusalOutcome(string Message, int ConflictCount, bool Untouched);

        private sealed record AcceptOutcome(string Message, Type HostType, Type KeyedType);

        private sealed record RegistryOutcome(string Rebound, string Removed, string Undeclared, int CleanCount);

        private sealed record ComposedOutcome(Type HostType, int FactoryRegistrations, string Rebound, string Removed);

        private sealed record NameOutcome(string Closed, string Open);
    }

    /// <summary>A sample seam.</summary>
    public interface IGreeter
    {
    }

    /// <summary>A sample seam implementation.</summary>
    public sealed class PoliteGreeter : IGreeter
    {
    }

    /// <summary>A second sample seam implementation.</summary>
    public sealed class CurtGreeter : IGreeter
    {
    }

    /// <summary>A sample default the host adds.</summary>
    public sealed class DefaultGreeter : IGreeter
    {
    }

    /// <summary>A sample catalog service.</summary>
    public interface IStep
    {
    }

    /// <summary>A sample catalog entry.</summary>
    public sealed class StepA : IStep
    {
    }

    /// <summary>A second sample catalog entry.</summary>
    public sealed class StepB : IStep
    {
    }

    /// <summary>A sample composed service.</summary>
    public interface IPipeline
    {
    }

    /// <summary>A sample composed implementation.</summary>
    public sealed class Pipeline : IPipeline
    {
    }

    /// <summary>A sample generic service, for name formatting.</summary>
    /// <typeparam name="T">Anything.</typeparam>
    public interface IHolder<T>
    {
    }
}
