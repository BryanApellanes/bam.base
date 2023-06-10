using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Serialization
{
    public class Serialized<T> : Serialized
    {
        public static implicit operator T(Serialized<T> serialized)
        {
            return serialized.Deserialize();
        }

        public Serialized() { }
        public Serialized(T data) : this(data, SerializationFormat.Json)
        {
        }

        public Serialized(T data, SerializationFormat format) : base(data, format)
        { 
        }

        public T Deserialize()
        {
            return (T)Serialization.Deserialize(this.Data, typeof(T), this.Format);
        }
    }
}
