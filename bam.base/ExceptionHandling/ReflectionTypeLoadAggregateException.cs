using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bam.ExceptionHandling
{
    public class ReflectionTypeLoadAggregateException: Exception
    {
        public ReflectionTypeLoadAggregateException(ReflectionTypeLoadException actual) : base(actual.LoaderExceptions.Aggregate("\r\n", (s, e) => $"{s}{e.Message}\r\n"), actual)
        { }
    }
}
