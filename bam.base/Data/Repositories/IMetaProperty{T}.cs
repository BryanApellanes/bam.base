namespace Bam.Net.Data.Repositories
{
    public interface IMetaProperty<T> : IMetaProperty
    {
        T TypedValue { get; }
    }
}