namespace Bam.Data.Repositories
{
    public interface IHashedObjectReader
    {
        T ReadByHash<T>(string hash);
        object ReadByHash(Type type, string hash);
    }
}
