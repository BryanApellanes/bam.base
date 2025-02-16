/*
	Copyright © Bryan Apellanes 2015  
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bam
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HashAlgorithms
    {
        Invalid,
        MD5,
        RIPEMD160,
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }
}
