using Bam.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Bam
{
    public class Serialized
    {
        public Serialized() { }
        public Serialized(object data): this(data, SerializationFormat.Json)
        {
        }

        public Serialized(object data, SerializationFormat format)
        {
            Args.ThrowIfNull(data, nameof(data));
            this.Type = data.GetType();
            this.Format = format;
            this.Data = Serialization.Serialization.Serialize(data, format);
        }

        public Type? Type { get; set; }

        public byte[]? Data { get; set; }

        public SerializationFormat Format { get; set; }

        public int Size
        {
            get => Data.Length;
        }

        public virtual object? Deserialize() 
        {
            return Serialization.Serialization.Deserialize(this.Data, this.Type, this.Format);
        }
    }
}
