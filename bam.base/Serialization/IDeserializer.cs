namespace Bam
{
    public interface IDeserializer
    {

        object? Deserialize(byte[] data, Type type);
    }
}
