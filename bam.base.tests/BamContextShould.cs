using Bam.Console;
using Bam.Net;
using Bam.Net.CoreServices;
using Bam.Net.Logging;
using Bam.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Tests
{
    [UnitTestMenu("DaoRepository Should", Selector = "drs")]
    public class BamContextShould : UnitTestMenuContainer
    {
        public BamContextShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        [UnitTest]
        public void GetServiceRegistryForCurrentProcessMode()
        {
            ProcessMode currentMode = ProcessMode.Current;
            ProcessModeServiceRegistry processModeServiceRegistry = BamContext.GetServiceRegistry();

            processModeServiceRegistry.ProcessMode.ShouldBeEqualTo(currentMode.Mode, "ProcessMode didn't match");

            Message.PrintLine("Current ProcessMode is {0}", currentMode.Mode);
        }

        [UnitTest]
        public void ConfigureByProcessMode()
        {
            BamContext.Configure(ProcessModes.Dev, (svcRegistry) =>
            {
                return svcRegistry
                .For<ILogger>().Use<ConsoleLogger>();
            });

            BamContext.Configure(ProcessModes.Test, (svcRegistry) =>
            {
                return svcRegistry
                .For<ILogger>().Use<TextFileLogger>();
            });

            ProcessModeServiceRegistry devServiceRegistry = BamContext.GetServiceRegistry(ProcessMode.Dev);

            ILogger devLogger = devServiceRegistry.ServiceRegistry.Get<ILogger>();
            devLogger.ShouldBeOfType<ConsoleLogger>();

            ProcessModeServiceRegistry testServiceRegistry = BamContext.GetServiceRegistry(ProcessMode.Test);

            ILogger testLogger = testServiceRegistry.ServiceRegistry.Get<ILogger>();
            testLogger.ShouldBeOfType<TextFileLogger>();
        }

        [UnitTest(DisplayName = "configure current mode")]
        public void ConfigureCurrentMode()
        {
            ProcessMode currentMode = ProcessMode.Current;
            currentMode.Mode.ShouldNotBeEqualTo(ProcessModes.Prod);

            BamContext.Configure(svcRegistry =>
            {
                return svcRegistry
                 .For<ITestClass>().Use<TestClass>();
            });

            Expect.Throws(() =>
            {
                BamContext.GetServiceRegistry(ProcessMode.Prod).ServiceRegistry.Get<ITestClass>();
            }, "No exception was thrown but it should have been");

            ProcessModeServiceRegistry processModeServiceRegistry = BamContext.GetServiceRegistry();
            processModeServiceRegistry.ProcessMode.ShouldNotBeNull();
            processModeServiceRegistry.ProcessMode.ShouldNotBeEqualTo(ProcessModes.Prod);
            processModeServiceRegistry.ProcessMode.ShouldBeEqualTo(currentMode.Mode);
            processModeServiceRegistry.ServiceRegistry.Get<ITestClass>().ShouldBeOfType<TestClass>();
        }
    }
}
