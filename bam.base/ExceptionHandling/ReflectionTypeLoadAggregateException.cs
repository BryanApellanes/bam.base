using System.Reflection;

namespace Bam.ExceptionHandling
{
    public class ReflectionTypeLoadAggregateException: Exception
    {
        public ReflectionTypeLoadAggregateException(ReflectionTypeLoadException actual) : base(actual.LoaderExceptions.Aggregate("\r\n", (s, e) => $"{s}{e.Message}\r\n"), actual)
        { }
    }
}
