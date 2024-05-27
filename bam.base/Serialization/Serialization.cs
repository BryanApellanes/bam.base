using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Serialization
{
    internal static class Serialization
    {
        static Dictionary<SerializationFormat, ISerializer> _serializers = new Dictionary<SerializationFormat, ISerializer>();
        static Dictionary<SerializationFormat, IDeserializer> _deserializers = new Dictionary<SerializationFormat, IDeserializer>();
        static Serialization()
        {
            _serializers.Add(SerializationFormat.Json, new JsonSerializer());
            _serializers.Add(SerializationFormat.Yaml, new YamlSerializer());
            _serializers.Add(SerializationFormat.Xml, new XmlSerializer());

            _deserializers.Add(SerializationFormat.Json, new JsonDeserializer());
            _deserializers.Add(SerializationFormat.Yaml, new YamlDeserializer());
            _deserializers.Add(SerializationFormat.Xml, new XmlDeserializer());
        }

        public static byte[] Serialize(object data, SerializationFormat format)
        {
            return _serializers[format].Serialize(data);
        }

        public static object? Deserialize(byte[] data, Type type, SerializationFormat format)
        {
            return _deserializers[format].Deserialize(data, type);
        }
    }
}
