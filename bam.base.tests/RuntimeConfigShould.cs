using Bam.Console;
using Bam.DependencyInjection;
using Bam.Services;
using Bam.Test;

namespace Bam.Tests;

[UnitTestMenu("RuntimeConfig should")]
public class RuntimeConfigShould : UnitTestMenuContainer
{
    public RuntimeConfigShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public async Task WriteDefault()
    {
        When.A<string>("writes default runtime config",
            () => RuntimeConfig.WriteDefault(),
            (result) => result)
        .TheTest
        .ShouldPass(because =>
        {
            because.TheResult.IsNotNull();
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
