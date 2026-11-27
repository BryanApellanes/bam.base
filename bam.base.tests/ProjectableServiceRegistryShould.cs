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

        [UnitTest]
        public void ProjectAServiceComposedAsItselfAfterItIsResolved()
        {
            When.A<ProjectableServiceRegistry>("composes Pipeline as itself, by type and by factory, resolves both and projects",
                () => new ProjectableServiceRegistry().Compose<Pipeline, Pipeline>().Compose<StepA>(registry => new StepA(), provider => new StepA()),
                registry =>
                {
                    registry.Get<Pipeline>();
                    registry.Get<StepA>();
                    ServiceCollection services = new ServiceCollection();
                    string message = Refusal(() => registry.ProjectInto(services));
                    using ServiceProvider provider = services.BuildServiceProvider();
                    return new SelfComposedOutcome(message, provider.GetService<Pipeline>() is not null, provider.GetService<StepA>() is not null);
                })
                .TheTest
                .ShouldPass<SelfComposedOutcome>((because, outcome) =>
                {
                    because.ItsTrue($"resolving a self-composed service is no conflict ({outcome.Message})", outcome.Message.Length == 0);
                    because.ItsTrue("the type-composed service reaches the host", outcome.HostHasPipeline);
                    because.ItsTrue("and so does the factory-composed one", outcome.HostHasStep);
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void RefuseOneServiceDeclaredUnderTwoKindsInEitherOrder()
        {
            When.A<string>("declares one service as a seam and as composed, a catalog or an extension, in both orders",
                () => "Declared as",
                _ =>
                {
                    List<ProjectableServiceRegistry> registries = new List<ProjectableServiceRegistry>
                    {
                        new ProjectableServiceRegistry().UseSeam<IGreeter, PoliteGreeter>().Compose<IGreeter, CurtGreeter>(),
                        new ProjectableServiceRegistry().UseSeam<IGreeter, PoliteGreeter>().Compose<IGreeter>(registry => new CurtGreeter(), provider => new CurtGreeter()),
                        new ProjectableServiceRegistry().Compose<IGreeter, CurtGreeter>().UseSeam<IGreeter, PoliteGreeter>(),
                        new ProjectableServiceRegistry().UseSeam<IStep, StepA>().AddToCatalog<IStep, StepB>(),
                        new ProjectableServiceRegistry().AddToCatalog<IStep, StepB>().UseSeam<IStep, StepA>(),
                        new ProjectableServiceRegistry().Compose<IStep, StepA>().AddExtension(typeof(IStep), typeof(StepB)),
                    };
                    List<string> messages = new List<string>();
                    List<int> registered = new List<int>();
                    foreach (ProjectableServiceRegistry registry in registries)
                    {
                        ServiceCollection services = new ServiceCollection();
                        messages.Add(Refusal(() => registry.ProjectInto(services)));
                        registered.Add(services.Count);
                    }
                    ProjectableServiceRegistry shared = new ProjectableServiceRegistry().AddToCatalog<IStep, StepA>().AddExtension(typeof(IStep), typeof(StepB));
                    return new MixedOutcome(messages, registered, Refusal(() => shared.ProjectInto(new ServiceCollection())));
                })
                .TheTest
                .ShouldPass<MixedOutcome>((because, outcome) =>
                {
                    because.ItsTrue("every mixed declaration is refused", outcome.Messages.All(message => message.Contains(" is declared as a ", StringComparison.Ordinal)));
                    because.ItsTrue("seam then compose names both kinds", outcome.Messages[0].Contains($"a seam ({typeof(PoliteGreeter).FullName}) and as a composed service", StringComparison.Ordinal));
                    because.ItsTrue("seam and catalog name both kinds", outcome.Messages[3].Contains("and as a catalog", StringComparison.Ordinal) && outcome.Messages[4].Contains("and as a catalog", StringComparison.Ordinal));
                    because.ItsTrue("composed and extension name both kinds", outcome.Messages[5].Contains("a composed service and as an extension", StringComparison.Ordinal));
                    because.ItsTrue("nothing was registered", outcome.Registered.All(count => count == 0));
                    because.ItsTrue($"a catalog and an extension may share a service ({outcome.Shared})", outcome.Shared.Length == 0);
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void CompareComposedBindingsWithTheCollection()
        {
            When.A<ProjectionOptions>("composes IGreeter as CurtGreeter onto collections holding another type, the known default and the declared type",
                () => new ProjectionOptions { KnownDefaults = new Dictionary<Type, Type> { [typeof(IGreeter)] = typeof(DefaultGreeter) } },
                options =>
                {
                    ServiceCollection other = new ServiceCollection();
                    other.AddSingleton<IGreeter, PoliteGreeter>();
                    string refused = Refusal(() => new ProjectableServiceRegistry().Compose<IGreeter, CurtGreeter>().ProjectInto(other, options));
                    ServiceCollection factory = new ServiceCollection();
                    factory.AddSingleton<IGreeter, PoliteGreeter>();
                    string factoryRefused = Refusal(() => new ProjectableServiceRegistry().Compose<IGreeter>(registry => new CurtGreeter(), provider => new CurtGreeter()).ProjectInto(factory, options));
                    ServiceCollection defaults = new ServiceCollection();
                    defaults.AddSingleton<IGreeter, DefaultGreeter>();
                    new ProjectableServiceRegistry().Compose<IGreeter, CurtGreeter>().ProjectInto(defaults, options);
                    ServiceCollection same = new ServiceCollection();
                    same.AddSingleton<IGreeter, CurtGreeter>();
                    new ProjectableServiceRegistry().Compose<IGreeter, CurtGreeter>().ProjectInto(same, options);
                    ServiceCollection hostOwned = new ServiceCollection();
                    hostOwned.AddSingleton<IGreeter, PoliteGreeter>();
                    string hostOwnedRefused = Refusal(() => new ProjectableServiceRegistry().Compose<IGreeter>(registry => new CurtGreeter()).ProjectInto(hostOwned, options));
                    using ServiceProvider defaultsProvider = defaults.BuildServiceProvider();
                    using ServiceProvider sameProvider = same.BuildServiceProvider();
                    return new ComposedCollectionOutcome(refused, factoryRefused, defaultsProvider.GetRequiredService<IGreeter>().GetType(), same.Count, sameProvider.GetRequiredService<IGreeter>().GetType(), hostOwnedRefused);
                })
                .TheTest
                .ShouldPass<ComposedCollectionOutcome>((because, outcome) =>
                {
                    because.ItsTrue("another type in the collection is refused", outcome.Refused.Contains($"is registered as {typeof(PoliteGreeter).FullName}, and the registry composes it as {typeof(CurtGreeter).FullName}", StringComparison.Ordinal));
                    because.ItsTrue("and so it is for a host factory", outcome.FactoryRefused.Contains("the registry composes it as a factory", StringComparison.Ordinal));
                    because.ItsTrue("the known default is replaced by the composed type", outcome.DefaultsHostType == typeof(CurtGreeter));
                    because.ItsTrue("the declared type already there stays one registration", outcome.SameCount == 1 && outcome.SameHostType == typeof(CurtGreeter));
                    because.ItsTrue("a composed binding the host registers itself isn't checked", outcome.HostOwnedRefused.Length == 0);
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void UseTheHostsWordingAndReportARebindThroughInclude()
        {
            When.A<ProjectionOptions>("words conflicts the way a host asks and rebinds a seam through Include",
                () => new ProjectionOptions
                {
                    RegistryName = "gateway",
                    RegistryConflictHeader = "holds a seam bound other than through its Use methods, which the service collection would not get.",
                    DeclarationName = "Use declaration",
                    DeclarationHints = new Dictionary<Type, string> { [typeof(IStep)] = "registry.UseStep<T>()" },
                },
                options =>
                {
                    ProjectableServiceRegistry undeclared = new ProjectableServiceRegistry();
                    undeclared.For<IStep>().Use<StepA>();
                    ServiceRegistry other = new ServiceRegistry();
                    other.For<IGreeter>().Use<CurtGreeter>();
                    ProjectableServiceRegistry included = new ProjectableServiceRegistry().UseSeam<IGreeter, PoliteGreeter>();
                    included.Include(other);
                    return new WordingOutcome(
                        Refusal(() => undeclared.ProjectInto(new ServiceCollection(), options)),
                        Refusal(() => included.ProjectInto(new ServiceCollection(), options)));
                })
                .TheTest
                .ShouldPass<WordingOutcome>((because, outcome) =>
                {
                    because.ItsTrue("the header is the host's", outcome.Undeclared.StartsWith("The gateway registry holds a seam bound other than through its Use methods, which the service collection would not get. ", StringComparison.Ordinal));
                    because.ItsTrue("and so is the declaration's name", outcome.Undeclared.Contains("without a Use declaration", StringComparison.Ordinal));
                    because.ItsTrue("a rebind through Include is refused", outcome.Included.Contains("then bound again as a factory (For<I>().Use..., Set with a factory, or Include/CopyFrom", StringComparison.Ordinal));
                })
                .SoBeHappy()
                .UnlessItFailed();
        }

        [UnitTest]
        public void ReadNullDictionariesAsEmptyAndNameArrays()
        {
            When.A<ProjectionOptions>("projects with every dictionary set to null and names an array of a generic type",
                () => new ProjectionOptions { KnownDefaults = null!, DeclarationHints = null!, ComposedAdvice = null!, RegistryConflictHeader = null!, DeclarationName = " " },
                options =>
                {
                    ServiceCollection services = new ServiceCollection();
                    services.AddSingleton<IGreeter, CurtGreeter>();
                    string message = Refusal(() => new ProjectableServiceRegistry().UseSeam<IGreeter, PoliteGreeter>().ProjectInto(services, options));
                    return new NullOptionsOutcome(message, options.RegistryConflictHeader, options.DeclarationName, ProjectableServiceRegistry.DisplayName(typeof(List<int>[])), ProjectableServiceRegistry.DisplayName(typeof(int[,])));
                })
                .TheTest
                .ShouldPass<NullOptionsOutcome>((because, outcome) =>
                {
                    because.ItsTrue($"a null dictionary reads as empty, so the conflict is still reported ({outcome.Message})", outcome.Message.Contains($"is registered as {typeof(CurtGreeter).FullName}", StringComparison.Ordinal));
                    because.ItsTrue("blank wording falls back to the defaults", outcome.Header == ProjectionOptions.DefaultRegistryConflictHeader && outcome.DeclarationName == ProjectionOptions.DefaultDeclarationName);
                    because.ItsTrue($"an array of a generic type is written as C# does ({outcome.GenericArray})", outcome.GenericArray == "System.Collections.Generic.List<System.Int32>[]");
                    because.ItsTrue($"and a two-dimensional array too ({outcome.MultiArray})", outcome.MultiArray == "System.Int32[,]");
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

        private sealed record SelfComposedOutcome(string Message, bool HostHasPipeline, bool HostHasStep);

        private sealed record MixedOutcome(List<string> Messages, List<int> Registered, string Shared);

        private sealed record ComposedCollectionOutcome(string Refused, string FactoryRefused, Type DefaultsHostType, int SameCount, Type SameHostType, string HostOwnedRefused);

        private sealed record WordingOutcome(string Undeclared, string Included);

        private sealed record NullOptionsOutcome(string Message, string Header, string DeclarationName, string GenericArray, string MultiArray);
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
