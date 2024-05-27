using Bam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
