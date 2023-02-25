namespace Bam.Net.Data.Repositories
{
    public interface IKeyedAuditRepoData : ICompositeKeyAuditRepoData
    {
        ulong Key { get; set; }

        T LoadByKey<T>(IRepository repository) where T : IKeyedAuditRepoData, new();
        T SaveByKey<T>(IRepository repository) where T : IKeyedAuditRepoData, new();
    }
}