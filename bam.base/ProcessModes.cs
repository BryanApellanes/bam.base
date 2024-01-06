using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
