namespace Bam.Data
{
    public interface IUniversalIdResolver
    {
        ulong GetId(object data);
    }
}