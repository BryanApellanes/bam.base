namespace Bam.Data.Repositories
{
    public interface IMeta<T> : IMeta
    {
        T TypedData { get; set; }
    }
}