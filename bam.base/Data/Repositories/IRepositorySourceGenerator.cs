using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Data.Repositories
{
    public interface IRepositorySourceGenerator: ISourceGenerator
    {
        void GenerateRepositorySource();
    }
}
