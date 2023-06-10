using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Serialization
{
    internal class XmlDeserializer : IDeserializer
    {
        public object? Deserialize(byte[] data, Type type)
        {
            System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(type);
            MemoryStream memoryStream = new MemoryStream(data);
            return xmlSerializer.Deserialize(memoryStream);
        }
    }
}
