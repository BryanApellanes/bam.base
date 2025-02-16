namespace Bam
{
    public interface IObjectEncoder
    {
        IObjectEncoding Encode(object data);
    }
}
