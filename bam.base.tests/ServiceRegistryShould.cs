using Bam.Net;
using Bam.Net.CoreServices;
using Bam.Shell;
using Bam.Testing;
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
    }
}
