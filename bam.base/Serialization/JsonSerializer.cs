using Newtonsoft.Json;
using System.Text;

namespace Bam.Serialization
{
    internal class JsonSerializer : ISerializer
    {
        public JsonSerializer() 
        {
            this.Formatting = Newtonsoft.Json.Formatting.Indented;
        }

        public Newtonsoft.Json.Formatting Formatting { get; set; }

        public JsonSerializerSettings JsonSerializerSettingsSettings { get; set; } = null!;

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public byte[] Serialize(object data)
        {
            return Serialize(data, this.Formatting);
        }

        public byte[] Serialize(object data, Newtonsoft.Json.Formatting formatting)
        {
            return Encoding.GetBytes(JsonConvert.SerializeObject(data, formatting));
        }

        public byte[] Serialize(object data, JsonSerializerSettings settings)
        {
            return Encoding.GetBytes(JsonConvert.SerializeObject(data, settings));
        }
    }
}
