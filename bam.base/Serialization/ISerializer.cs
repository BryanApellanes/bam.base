namespace Bam
{
    public interface ISerializer
    {
        byte[] Serialize(object data);
    }
}
