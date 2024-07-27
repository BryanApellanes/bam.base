using Bam;
using Bam.CoreServices;
using Bam.Shell;
using Bam.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Tests
{
    [UnitTestMenu("ServiceRegistry Should", Selector = "srs")]
    public class ServiceRegistryShould
    {
        [UnitTest]
        public void UseSingleton()
        {
            ServiceRegistry svcRegistry = ServiceRegistry
                .Create()
                .For<ITestClass>().UseSingleton<TestClass>();

            ITestClass refOne = svcRegistry.Get<ITestClass>();
            ITestClass refTwo = svcRegistry.Get<ITestClass>();

            refOne.ShouldBe(refTwo);
            refOne.Name.ShouldBeEqualTo(refTwo.Name);
        }

        [UnitTest]
        public void UseTransient()
        {
            ServiceRegistry svcRegistry = ServiceRegistry
                .Create()
                .For<ITestClass>().UseTransient<TestClass>();

            ITestClass refOne = svcRegistry.Get<ITestClass>();
            ITestClass refTwo = svcRegistry.Get<ITestClass>();

            refOne.ShouldNotBe(refTwo);
            refOne.Name.ShouldNotBeEqualTo(refTwo.Name);
        }

        [UnitTest]
        public void ReplaceAType()
        {
            ServiceRegistry svcRegistry = ServiceRegistry
                .Create()
                .For<ITestClass>().Use<TestClass>()
                .For<ITestClass>().Use<DifferentTestClass>();

            ITestClass testClass = svcRegistry.Get<ITestClass>();
            testClass.GetType().ShouldEqual(typeof(DifferentTestClass));
        }

        [UnitTest]
        public void UseTheReplacedType()
        {
            // Test to ensure that specifying the same interface
            // multiple times causes the latest to be used 
            // when constructing a class
            ServiceRegistry svcRegistry = ServiceRegistry
                .Create()
                .For<DependentClass>().Use<DependentClass>()
                .For<ITestClass>().Use<TestClass>()
                .For<ITestClass>().Use<DifferentTestClass>();

            DependentClass instance = svcRegistry.Get<DependentClass>();
            instance.TestClass.ShouldNotBeNull();
            instance.TestClass.GetType().ShouldEqual(typeof(DifferentTestClass));
        }

        [UnitTest]
        public void ConstructTheRequestedClass()
        {
            // Test to ensure that it isn't necessary to 
            // specify a type in the registry as long as
            // the required constructor parameters are
            // in the registry.
            ServiceRegistry svcRegistry = ServiceRegistry
                .Create()
                .For<ITestClass>().Use<TestClass>()
                .For<ITestClass>().Use<DifferentTestClass>();

            DependentClass instance = svcRegistry.Get<DependentClass>();
            instance.TestClass.ShouldNotBeNull();
            instance.TestClass.GetType().ShouldEqual(typeof(DifferentTestClass));
        }
    }
}
