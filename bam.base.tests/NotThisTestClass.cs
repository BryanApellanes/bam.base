using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Tests
{
    public class NotThisTestClass : ITestClass
    {
        public string Name => throw new NotImplementedException();
    }
}
