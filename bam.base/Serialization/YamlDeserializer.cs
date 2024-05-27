using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Serialization
{
    internal class YamlDeserializer : IDeserializer
    {
        public YamlDeserializer() 
        {
            Encoding = Encoding.UTF8;
        }
        public Encoding Encoding { get; set; }
        public object? Deserialize(byte[] data, Type type)
        {
            YamlDotNet.Serialization.Deserializer deserializer = new YamlDotNet.Serialization.Deserializer();
            string yaml = Encoding.GetString(data, 0, data.Length);
            return deserializer.Deserialize(yaml, type);
        }
    }
}
