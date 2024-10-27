//using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bam
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PipelineFactoryTransformerNameAttribute : Attribute
    {
        public PipelineFactoryTransformerNameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}
