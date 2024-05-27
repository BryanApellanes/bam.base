using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Serialization
{
    internal class JsonDeserializer : IDeserializer
    {
        public JsonDeserializer()
        {
            this.Formatting = Newtonsoft.Json.Formatting.Indented;
        }

        public Newtonsoft.Json.Formatting Formatting { get; set; }

        public JsonSerializerSettings JsonSerializerSettingsSettings { get; set; }

        public Encoding Encoding { get; set; }

        public object? Deserialize(byte[] data, Type type)
        {
            string json = Encoding.GetString(data, 0, data.Length);
            return JsonConvert.DeserializeObject(json, type);
        }
    }
}
