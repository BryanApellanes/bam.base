namespace Bam.Data.Repositories
{
    public interface IStorableTypesProvider
    {
        void AddTypes(IRepository repository);
        HashSet<Type> GetTypes();
    }
}
