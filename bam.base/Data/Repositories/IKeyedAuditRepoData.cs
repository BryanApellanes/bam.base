namespace Bam.Data.Repositories
{
    public interface IKeyedAuditRepoData : ICompositeKeyAuditRepoData
    {
        new ulong Key { get; set; } 
        T LoadByKey<T>(IRepository repository) where T : class, new();
        T SaveByKey<T>(IRepository repository) where T : class, new();
    }
}