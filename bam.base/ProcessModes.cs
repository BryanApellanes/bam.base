using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Bam
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProcessModes
    {
        Dev,
        Test,
        Prod
    }
}
