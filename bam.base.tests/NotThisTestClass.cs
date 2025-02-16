namespace Bam.Tests
{
    public class NotThisTestClass : ITestClass
    {
        public string Name => throw new NotImplementedException();
    }
}
