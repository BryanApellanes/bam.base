using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Data.Repositories
{
    public enum CrudMethods
    {
        Invalid,
        Create,
        Retrieve,
        Update,
        Delete,
        Query,
        SaveCollection
    }
}
