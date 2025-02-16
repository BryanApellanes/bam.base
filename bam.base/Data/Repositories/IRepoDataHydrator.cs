namespace Bam.Data.Repositories
{
    public interface IRepoDataHydrator
    {
        bool TryHydrate(IRepoData data, IRepository repository);
        void Hydrate(IRepoData data, IRepository repository);
    }
}
