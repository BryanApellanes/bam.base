/*
	Copyright © Bryan Apellanes 2015  
*/

using Bam.Services;

namespace Bam.Data
{
    public interface IRegistrarCaller
    {
        void Register(IDatabase database);
        void Register(string connectionName);
        void Register(Type daoType);
        void Register<T>() where T : IDao;
        void Register(DependencyProvider incubator);
    }
}
