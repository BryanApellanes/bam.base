//using Newtonsoft.Json.Schema.Generation;

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
