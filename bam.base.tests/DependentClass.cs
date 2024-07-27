namespace Bam.Tests;

public class DependentClass
{
    public DependentClass(ITestClass testClass)
    {
        this.TestClass = testClass;
    }
    
    public ITestClass TestClass { get; }
}