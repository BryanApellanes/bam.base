using Bam.Test;
using Bam.Console;
using Bam.DependencyInjection;
using Bam.Services;
using Bam.Tests.TestClasses;

namespace Bam.Tests
{
    [UnitTestMenu("ServiceRegistry Should", Selector = "srs")]
    public class ServiceRegistryShould
    {
        [UnitTest]
        public void UseSingleton()
        {
            When.A<ServiceRegistry>("uses singleton registration",
                () => ServiceRegistry.Create().For<ITestClass>().UseSingleton<TestClass>(),
                (svcRegistry) =>
                {
                    ITestClass refOne = svcRegistry.Get<ITestClass>();
                    ITestClass refTwo = svcRegistry.Get<ITestClass>();
                    return new object[] { refOne, refTwo };
                })
            .TheTest
            .ShouldPass(because =>
            {
                object[] results = (object[])because.Result;
                ITestClass refOne = (ITestClass)results[0];
                ITestClass refTwo = (ITestClass)results[1];
                because.ItsTrue("references are the same", ReferenceEquals(refOne, refTwo));
                because.ItsTrue("names are equal", refOne.Name.Equals(refTwo.Name));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void UseTransient()
        {
            When.A<ServiceRegistry>("uses transient registration",
                () => ServiceRegistry.Create().For<ITestClass>().UseTransient<TestClass>(),
                (svcRegistry) =>
                {
                    ITestClass refOne = svcRegistry.Get<ITestClass>();
                    ITestClass refTwo = svcRegistry.Get<ITestClass>();
                    return new object[] { refOne, refTwo };
                })
            .TheTest
            .ShouldPass(because =>
            {
                object[] results = (object[])because.Result;
                ITestClass refOne = (ITestClass)results[0];
                ITestClass refTwo = (ITestClass)results[1];
                because.ItsTrue("references are different", !ReferenceEquals(refOne, refTwo));
                because.ItsTrue("names are different", !refOne.Name.Equals(refTwo.Name));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void ReplaceAType()
        {
            When.A<ServiceRegistry>("replaces a type registration",
                () => ServiceRegistry.Create()
                    .For<ITestClass>().Use<TestClass>()
                    .For<ITestClass>().Use<DifferentTestClass>(),
                (svcRegistry) => svcRegistry.Get<ITestClass>())
            .TheTest
            .ShouldPass(because =>
            {
                because.ItsTrue("resolved type is DifferentTestClass", because.Result?.GetType() == typeof(DifferentTestClass));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void UseTheReplacedType()
        {
            When.A<ServiceRegistry>("uses the replaced type when constructing",
                () => ServiceRegistry.Create()
                    .For<DependentClass>().Use<DependentClass>()
                    .For<ITestClass>().Use<TestClass>()
                    .For<ITestClass>().Use<DifferentTestClass>(),
                (svcRegistry) => svcRegistry.Get<DependentClass>())
            .TheTest
            .ShouldPass(because =>
            {
                because.TheResult.IsNotNull()
                    .As<DependentClass>("TestClass is not null", d => d?.TestClass != null)
                    .As<DependentClass>("TestClass is DifferentTestClass", d => d?.TestClass?.GetType() == typeof(DifferentTestClass));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void ConstructTheRequestedClass()
        {
            When.A<ServiceRegistry>("constructs the requested class without explicit registration",
                () => ServiceRegistry.Create()
                    .For<ITestClass>().Use<TestClass>()
                    .For<ITestClass>().Use<DifferentTestClass>(),
                (svcRegistry) => svcRegistry.Get<DependentClass>())
            .TheTest
            .ShouldPass(because =>
            {
                because.TheResult.IsNotNull()
                    .As<DependentClass>("TestClass is not null", d => d?.TestClass != null)
                    .As<DependentClass>("TestClass is DifferentTestClass", d => d?.TestClass?.GetType() == typeof(DifferentTestClass));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void DetectWideDependencyLoop()
        {
            When.A<ServiceRegistry>("Tries to construct a class with a dependency loop", svcRegistry =>
            {
                svcRegistry.Get<FirstClass>();
            })
            .ExpectException(true)
            .TheTest
            .ShouldPass(because =>
            {
                because.TheTestCase("threw an exception", (testCase) => testCase.Exception != null);
                because.TheTestCase($"threw an exception of type {nameof(DependencyLoopException)}", (testCase) => testCase.Exception?.GetType() == typeof(DependencyLoopException));
                because.AdditionalInformation($"loop was successfully detected: {because.TestCase?.Exception?.Message}");
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void DetectTightDependencyLoop()
        {
            When.A<ServiceRegistry>("Tries to construct a class with a dependency loop", svcRegistry =>
            {
                svcRegistry.Get<ClassA>();
            })
            .ExpectException(true)
            .TheTest
            .ShouldPass(because =>
            {
                because.TheTestCase("threw an exception", (testCase) => testCase.Exception != null);
                because.TheTestCase($"threw an exception of type {nameof(DependencyLoopException)}", (testCase) => testCase.Exception?.GetType() == typeof(DependencyLoopException));
                because.AdditionalInformation($"loop was successfully detected: {because.TestCase?.Exception?.Message}");
            })
            .SoBeHappy()
            .UnlessItFailed();
        }
    }
}
