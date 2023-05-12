namespace Bam.Net.Data.Repositories
{
    public interface ICompositeKeyAuditRepoData :IAuditRepoData
    {
        string CompositeKey { get; set; }
        HashAlgorithms CompositeKeyAlgorithm { get; set; }
        ulong CompositeKeyId { get; set; }
        string[] CompositeKeyProperties { get; set; }

        bool Equals(object obj);
        bool ExistsIn<T>(IRepository repository, out T existing) where T : class, new();
        string GetCompositeKeyString(HashAlgorithms algorithm = HashAlgorithms.SHA256);
        int GetHashCode();
        int GetIntKeyHash();
        long GetLongKeyHash();
        ulong GetULongKeyHash();
        ulong Key();
        T LoadByCompositeKey<T>(IRepository repository) where T : class, new();
        T LoadByCompositeKeyId<T>(IRepository repository) where T : class, new();
    }
}