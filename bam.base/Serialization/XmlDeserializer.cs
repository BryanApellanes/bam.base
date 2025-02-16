namespace Bam.Serialization
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
