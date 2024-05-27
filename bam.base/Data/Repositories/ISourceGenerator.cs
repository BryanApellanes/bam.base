using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Data.Repositories
{
    public interface ISourceGenerator
    {
        void GenerateSource(string writeSourceTo);
    }
}
