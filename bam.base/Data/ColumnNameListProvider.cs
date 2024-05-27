using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Data
{
    public delegate string ColumnNameListProvider(Type type, IDatabase db);
}
