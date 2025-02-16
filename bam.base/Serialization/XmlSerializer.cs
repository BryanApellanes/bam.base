namespace Bam.Serialization
{
    internal class XmlSerializer : ISerializer
    {        
        public byte[] Serialize(object data)
        {
            System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(data.GetType());
            MemoryStream memoryStream = new MemoryStream();
            xmlSerializer.Serialize(memoryStream, data);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream.GetBuffer();
        }
    }
}
