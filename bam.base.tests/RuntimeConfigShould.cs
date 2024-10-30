using Bam.Console;
using Bam.CoreServices;
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
        Message.PrintLine(RuntimeConfig.WriteDefault());
    }
}