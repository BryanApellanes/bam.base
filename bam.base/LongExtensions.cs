using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net
{
    public static class LongExtensions
    { 
        public static ulong MapToUlong(this long longValue)
        {
            return unchecked((ulong)(longValue - long.MinValue));
        }
    }
}
