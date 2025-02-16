using System.Reflection;

namespace Bam.Data.Repositories
{
    public interface IObjectPersisterDirectoryProvider
    {
        string RootDirectory { get; set; }
        DirectoryInfo GetTypeDirectory(Type type);
        DirectoryInfo GetPropertyDirectory(PropertyInfo prop);
        DirectoryInfo GetPropertyDirectory(Type type, PropertyInfo prop);
    }
}
