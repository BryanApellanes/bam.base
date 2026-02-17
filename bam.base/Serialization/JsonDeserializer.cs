using Newtonsoft.Json;
using System.Text;

namespace Bam.Serialization
{
    internal class JsonDeserializer : IDeserializer
    {
        public JsonDeserializer()
        {
            this.Formatting = Newtonsoft.Json.Formatting.Indented;
        }

        public Newtonsoft.Json.Formatting Formatting { get; set; }

        public JsonSerializerSettings JsonSerializerSettingsSettings { get; set; } = null!;

        public Encoding Encoding { get; set; } = null!;

        public object? Deserialize(byte[] data, Type type)
        {
            string json = Encoding.GetString(data, 0, data.Length);
            return JsonConvert.DeserializeObject(json, type);
        }
    }
}
