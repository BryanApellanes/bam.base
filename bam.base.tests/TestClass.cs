namespace Bam.Tests
{
    public class TestClass : ITestClass
    {
        public TestClass()
        {
            this.Name = 16.RandomLetters();
        }

        public string Name
        {
            get;
            private set;
        }
    }
}
