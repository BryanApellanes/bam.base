using System.Text;

namespace Bam.Serialization
{
    internal class YamlSerializer : ISerializer
    {
        public YamlSerializer() 
        {
            Encoding = Encoding.UTF8;
        }
        
        public Encoding Encoding { get; set; }

        public byte[] Serialize(object data)
        {
            YamlDotNet.Serialization.Serializer serializer = new YamlDotNet.Serialization.Serializer();
            return Encoding.GetBytes(serializer.Serialize(data));
        }
    }
}
