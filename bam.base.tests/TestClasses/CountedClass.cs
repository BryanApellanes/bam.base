namespace Bam.Tests.TestClasses
{
    public interface ICountedClass
    {
        int Id { get; }
    }

    /// <summary>
    /// Counts its own constructions, so a test can tell how many times a registration was resolved to answer
    /// one question.
    /// </summary>
    public class CountedClass : ICountedClass
    {
        private static int _constructed;

        public CountedClass()
        {
            Id = Interlocked.Increment(ref _constructed);
        }

        public static int Constructed => _constructed;

        public int Id { get; }
    }
}

namespace Bam.Tests.TestClasses
{
    /// <summary>Registered with a factory that returns null, to show that contained means registered.</summary>
    public interface INeverResolved
    {
    }
}
