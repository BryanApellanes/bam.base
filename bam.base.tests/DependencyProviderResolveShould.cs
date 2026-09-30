using Bam.DependencyInjection;
using Bam.Test;
using Bam.Tests.TestClasses;

namespace Bam.Tests
{
    /// <summary>
    /// A registration that is a factory runs each time it is read. These tests pin that resolving a service
    /// reads its registration once, so a transient service is constructed once per resolve.
    /// </summary>
    [UnitTestMenu("DependencyProvider resolve Should", Selector = "dprs")]
    public class DependencyProviderResolveShould
    {
        [UnitTest]
        public void ConstructATransientServiceOncePerResolve()
        {
            After.Setup(reg =>
            {
                reg.For<ServiceRegistry>().Use(ServiceRegistry.Create().For<ICountedClass>().Use<CountedClass>());
            })
            .When<ServiceRegistry>("resolves a transient service twice", registry =>
            {
                int before = CountedClass.Constructed;
                ICountedClass first = registry.Get<ICountedClass>();
                int afterFirst = CountedClass.Constructed;
                ICountedClass second = registry.Get<ICountedClass>();
                return new ResolveOutcome(afterFirst - before, CountedClass.Constructed - afterFirst, first.Id != second.Id);
            })
            .TheTest
            .ShouldPass<ResolveOutcome>((because, outcome) =>
            {
                because.ItsTrue("the first resolve constructed one instance", outcome.First == 1, $"constructed: {outcome.First}");
                because.ItsTrue("the second resolve constructed one instance", outcome.Second == 1, $"constructed: {outcome.Second}");
                because.ItsTrue("each resolve returned its own instance", outcome.Distinct);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void InvokeAFactoryOncePerResolve()
        {
            int invoked = 0;
            int invokedWithRegistry = 0;

            After.Setup(reg =>
            {
                ServiceRegistry registry = ServiceRegistry.Create();
                registry.For<ICountedClass>().Use<CountedClass>(() =>
                {
                    invoked++;
                    return new CountedClass();
                });
                registry.For<ITestClass>().Use<TestClass>(provider =>
                {
                    invokedWithRegistry++;
                    return new TestClass();
                });
                reg.For<ServiceRegistry>().Use(registry);
            })
            .When<ServiceRegistry>("resolves services registered as factories", registry =>
            {
                registry.Get<ICountedClass>();
                int afterOne = invoked;
                registry.Get<ICountedClass>();
                registry.Get<ITestClass>();
                return new ResolveOutcome(afterOne, invoked - afterOne, invokedWithRegistry == 1);
            })
            .TheTest
            .ShouldPass<ResolveOutcome>((because, outcome) =>
            {
                because.ItsTrue("the first resolve invoked the factory once", outcome.First == 1, $"invoked: {outcome.First}");
                because.ItsTrue("the second resolve invoked it once more", outcome.Second == 1, $"invoked: {outcome.Second}");
                because.ItsTrue("a factory that takes the registry is invoked once too", outcome.Distinct);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void ResolveOnceThroughEveryGet()
        {
            After.Setup(reg =>
            {
                reg.For<ServiceRegistry>().Use(ServiceRegistry.Create().For<ICountedClass>().Use<CountedClass>());
            })
            .When<ServiceRegistry>("resolves a transient service through each overload", registry =>
            {
                int byType = Constructions(() => registry.Get(typeof(ICountedClass)));
                int byTryGet = Constructions(() => registry.TryGet(out ICountedClass ignored));
                int byTryGetType = Constructions(() => registry.TryGet(typeof(ICountedClass), out object ignored));
                int byCtorParams = Constructions(() => registry.Get<ICountedClass>(new object[] { }));
                int byCtorTypes = Constructions(() => registry.Get<ICountedClass>(Type.EmptyTypes));
                int byTypeAndCtorTypes = Constructions(() => registry.Get(typeof(ICountedClass), Type.EmptyTypes));
                int byDefault = Constructions(() => registry.Get<ICountedClass>(new CountedClassStandIn()));
                return new OverloadOutcome(byType, byTryGet, byTryGetType, byCtorParams, byCtorTypes, byTypeAndCtorTypes, byDefault);
            })
            .TheTest
            .ShouldPass<OverloadOutcome>((because, outcome) =>
            {
                because.ItsTrue("Get(Type) constructs once", outcome.ByType == 1, $"constructed: {outcome.ByType}");
                because.ItsTrue("TryGet<T> constructs once", outcome.ByTryGet == 1, $"constructed: {outcome.ByTryGet}");
                because.ItsTrue("TryGet(Type) constructs once", outcome.ByTryGetType == 1, $"constructed: {outcome.ByTryGetType}");
                because.ItsTrue("Get<T>(params object[]) constructs once", outcome.ByCtorParams == 1, $"constructed: {outcome.ByCtorParams}");
                because.ItsTrue("Get<T>(params Type[]) constructs once", outcome.ByCtorTypes == 1, $"constructed: {outcome.ByCtorTypes}");
                because.ItsTrue("Get(Type, params Type[]) constructs once", outcome.ByTypeAndCtorTypes == 1, $"constructed: {outcome.ByTypeAndCtorTypes}");
                because.ItsTrue("Get<T>(setToIfNull) constructs once and leaves the stand-in unused", outcome.ByDefault == 1, $"constructed: {outcome.ByDefault}");
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void AnswerContainsWithoutConstructing()
        {
            After.Setup(reg =>
            {
                reg.For<ServiceRegistry>().Use(ServiceRegistry.Create().For<ICountedClass>().Use<CountedClass>());
            })
            .When<ServiceRegistry>("is asked what it contains", registry =>
            {
                bool registered = false;
                bool unregistered = true;
                int constructed = Constructions(() =>
                {
                    registered = registry.Contains<ICountedClass>() && registry.Contains(typeof(ICountedClass));
                    unregistered = registry.Contains<ITestClass>();
                });
                return new ContainsOutcome(registered, unregistered, constructed);
            })
            .TheTest
            .ShouldPass<ContainsOutcome>((because, outcome) =>
            {
                because.ItsTrue("a registered factory is contained", outcome.Registered);
                because.ItsTrue("an unregistered type is not", !outcome.Unregistered);
                because.ItsTrue("nothing was constructed to find out", outcome.Constructed == 0, $"constructed: {outcome.Constructed}");
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void KeepASingletonSingle()
        {
            After.Setup(reg =>
            {
                reg.For<ServiceRegistry>().Use(ServiceRegistry.Create().For<ICountedClass>().UseSingleton<CountedClass>());
            })
            .When<ServiceRegistry>("resolves a singleton twice", registry =>
            {
                int before = CountedClass.Constructed;
                ICountedClass first = registry.Get<ICountedClass>();
                ICountedClass second = registry.Get<ICountedClass>();
                return new ResolveOutcome(CountedClass.Constructed - before, 0, ReferenceEquals(first, second));
            })
            .TheTest
            .ShouldPass<ResolveOutcome>((because, outcome) =>
            {
                because.ItsTrue("resolving constructed nothing new", outcome.First == 0, $"constructed: {outcome.First}");
                because.ItsTrue("both resolves returned the same instance", outcome.Distinct);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void ConstructAnUnregisteredClassOnceAndKeepIt()
        {
            After.Setup(reg =>
            {
                reg.For<ServiceRegistry>().Use(ServiceRegistry.Create());
            })
            .When<ServiceRegistry>("resolves a class that was never registered", registry =>
            {
                int before = CountedClass.Constructed;
                CountedClass first = registry.Get<CountedClass>();
                CountedClass second = registry.Get<CountedClass>();
                return new ResolveOutcome(CountedClass.Constructed - before, 0, ReferenceEquals(first, second));
            })
            .TheTest
            .ShouldPass<ResolveOutcome>((because, outcome) =>
            {
                because.ItsTrue("it is constructed once", outcome.First == 1, $"constructed: {outcome.First}");
                because.ItsTrue("and kept for later resolves", outcome.Distinct);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        private static int Constructions(Action action)
        {
            int before = CountedClass.Constructed;
            action();
            return CountedClass.Constructed - before;
        }

        /// <summary>Stands in as a default for Get(setToIfNull) without counting as a CountedClass construction.</summary>
        private sealed class CountedClassStandIn : ICountedClass
        {
            public int Id => -1;
        }

        private sealed record ResolveOutcome(int First, int Second, bool Distinct);

        private sealed record OverloadOutcome(int ByType, int ByTryGet, int ByTryGetType, int ByCtorParams, int ByCtorTypes, int ByTypeAndCtorTypes, int ByDefault);

        private sealed record ContainsOutcome(bool Registered, bool Unregistered, int Constructed);
    }
}
