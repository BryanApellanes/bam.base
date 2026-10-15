using Bam.DependencyInjection;
using Bam.Test;

namespace Bam.Tests;

/// <summary>Deliberately failing test that puts one red CI run on record for bam.base#29; reverted right after.</summary>
[UnitTestMenu("CI red path should")]
public class CiRedPathShould : UnitTestMenuContainer
{
    public CiRedPathShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void FailOnPurposeToProveTheGate()
    {
        When.A<string>("fails on purpose", () => "red", value => value)
            .TheTest
            .ShouldPass<string>((because, value) => because.ItsTrue("this assertion is false on purpose", value == "green"))
            .SoBeHappy()
            .UnlessItFailed();
    }
}
