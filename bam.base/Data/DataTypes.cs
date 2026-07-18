/*
	Copyright © Bryan Apellanes 2015  
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bam.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataTypes
    {
        Default,
        Boolean,
        Int,
        UInt,
        ULong,
        Long,
        Decimal,
        String,
        ByteArray,
        DateTime,
        Vector,
        Json,
        UuidArray
    }
}
