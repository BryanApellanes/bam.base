using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public interface ITypeResolver
    {
        Type ResolveType(string typeName);
        Type ResolveType(string nameSpace, string typeName);
    }
}
