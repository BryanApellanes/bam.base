using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Data.Repositories;

namespace Bam.Data.Repositories
{
    public interface IStorableTypesProvider
    {
        void AddTypes(IRepository repository);
        HashSet<Type> GetTypes();
    }
}
