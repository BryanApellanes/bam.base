using Bam.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Data.Repositories
{
    public interface IRepoDataHydrator
    {
        bool TryHydrate(IRepoData data, IRepository repository);
        void Hydrate(IRepoData data, IRepository repository);
    }
}
