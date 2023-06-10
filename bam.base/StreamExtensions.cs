using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using YamlDotNet.Serialization;

namespace Bam.Net
{
    public static class StreamExtensions
    {
        public static string ReadToEnd(this Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

        public static object FromXmlStream(this Stream xmlStream, Type type)
        {
            return new XmlSerializer(type).Deserialize(xmlStream);
        }

        public static object FromJsonStream(this Stream stream, Type type)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            using (StreamReader sr = new StreamReader(ms))
            {
                return sr.ReadToEnd().FromJson(type);
            }
        }

        public static object FromYamlStream(this Stream stream, Type type)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            Deserializer deserializer = new Deserializer();
            using (StreamReader sr = new StreamReader(ms))
            {
                return deserializer.Deserialize(sr, type);
            }
        }
    }
}
