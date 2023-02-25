/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net.Incubation;

namespace Bam.Net.Data
{
    public interface IRegistrarCaller
    {
        void Register(IDatabase database);
        void Register(string connectionName);
        void Register(Type daoType);
        void Register<T>() where T : IDao;
        void Register(Incubator incubator);
    }
}
