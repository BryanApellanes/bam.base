using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Bam.Base.Transformers;

public static class ObjectExtensions
{
    public static byte[] ToBson(this object value)
    {
        MemoryStream memoryStream = new MemoryStream();
        using(BsonDataWriter writer = new BsonDataWriter(memoryStream))
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Serialize(writer, value);
        }

        return memoryStream.ToArray();
    }
    
    public static T? FromBson<T>(this byte[] data)
    {
        MemoryStream memoryStream = new MemoryStream(data);
        using(BsonDataReader reader = new BsonDataReader(memoryStream))
        {
            JsonSerializer jsonSerializer = new JsonSerializer();

            return jsonSerializer.Deserialize<T>(reader);
        }
    }
}