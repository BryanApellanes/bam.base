using System;
using System.Collections.Generic;
using System.Text;

namespace Bam
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class PipelineFactoryConstructorAttribute : Attribute
    {
    }
}
