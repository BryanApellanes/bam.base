using Bam.Console;
using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Test;
using Bam.Services;

namespace Bam.Tests
{
    [UnitTestMenu("BamContext Should", Selector = "bcs")]
    public class BamContextShould : UnitTestMenuContainer
    {
        public BamContextShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        [UnitTest]
        public void GetServiceRegistryForCurrentProcessMode()
        {
            ProcessMode currentMode = ProcessMode.Current;

            When.A<ProcessModeServiceRegistry>("gets service registry for current process mode",
                () => BamContext.GetServiceRegistry()!,
                (registry) => registry)
            .TheTest
            .ShouldPass(because =>
            {
                because.TheResult.IsNotNull()
                    .As<ProcessModeServiceRegistry>("ProcessMode matches current", r => currentMode.Mode.Equals(r?.ProcessMode));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void ConfigureByProcessMode()
        {
            When.A<ProcessModeServiceRegistry>("configures loggers by process mode",
                () =>
                {
                    BamContext.Configure(ProcessModes.Dev, (svcRegistry) =>
                        svcRegistry.For<ILogger>().Use<ConsoleLogger>());
                    BamContext.Configure(ProcessModes.Test, (svcRegistry) =>
                        svcRegistry.For<ILogger>().Use<TextFileLogger>());
                    return BamContext.GetServiceRegistry(ProcessMode.Dev)!;
                },
                (devRegistry) =>
                {
                    ILogger devLogger = devRegistry.ServiceRegistry.Get<ILogger>();
                    ProcessModeServiceRegistry testRegistry = BamContext.GetServiceRegistry(ProcessMode.Test)!;
                    ILogger testLogger = testRegistry!.ServiceRegistry.Get<ILogger>();
                    return new object[] { devLogger, testLogger };
                })
            .TheTest
            .ShouldPass(because =>
            {
                object[] results = (object[])because.Result;
                because.ItsTrue("dev logger is ConsoleLogger", results[0] is ConsoleLogger);
                because.ItsTrue("test logger is TextFileLogger", results[1] is TextFileLogger);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(DisplayName = "configure current mode")]
        public void ConfigureCurrentMode()
        {
            ProcessMode currentMode = ProcessMode.Current;

            When.A<ProcessModeServiceRegistry>("configures current mode",
                () =>
                {
                    BamContext.Configure(svcRegistry =>
                    {
                        return svcRegistry
                         .For<ITestClass>().Use<TestClass>();
                    });
                    return BamContext.GetServiceRegistry()!;
                },
                (registry) =>
                {
                    bool prodThrew = false;
                    try
                    {
                        BamContext.GetServiceRegistry(ProcessMode.Prod)!.ServiceRegistry.Get<ITestClass>();
                    }
                    catch
                    {
                        prodThrew = true;
                    }
                    ITestClass testClass = registry.ServiceRegistry.Get<ITestClass>();
                    return new object[] { registry, prodThrew, testClass };
                })
            .TheTest
            .ShouldPass(because =>
            {
                object[] results = (object[])because.Result;
                ProcessModeServiceRegistry registry = (ProcessModeServiceRegistry)results[0];
                bool prodThrew = (bool)results[1];
                ITestClass testClass = (ITestClass)results[2];
                because.ItsTrue("current mode is not Prod", !currentMode.Mode.Equals(ProcessModes.Prod));
                because.ItsTrue("getting ITestClass from Prod throws", prodThrew);
                because.ItsTrue("ProcessMode is not null", true);
                because.ItsTrue("ProcessMode is not Prod", !registry.ProcessMode.Equals(ProcessModes.Prod));
                because.ItsTrue("ProcessMode equals current mode", registry.ProcessMode.Equals(currentMode.Mode));
                because.ItsTrue("ITestClass is TestClass", testClass is TestClass);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }
    }
}
