namespace Bam.Data.Repositories
{
    public interface IKeyedAuditRepoData : ICompositeKeyAuditRepoData
    {
        ulong Key { get; set; } // TODO: remove this

        T LoadByKey<T>(IRepository repository) where T : class, new();
        T SaveByKey<T>(IRepository repository) where T : class, new();
    }
}