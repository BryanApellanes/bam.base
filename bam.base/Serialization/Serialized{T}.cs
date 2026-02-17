namespace Bam.Serialization
{
    public class Serialized<T> : Serialized
    {
        public static implicit operator T(Serialized<T> serialized)
        {
            return serialized.Deserialize()!;
        }

        public Serialized() { }
        public Serialized(T data) : this(data, SerializationFormat.Json)
        {
        }

        public Serialized(T data, SerializationFormat format) : base(data!, format)
        { 
        }

        public new T? Deserialize()
        {
            return (T?)Serialization.Deserialize(this.Data!, typeof(T), this.Format);
        }
    }
}
