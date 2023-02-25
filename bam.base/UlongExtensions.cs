using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net
{
    public static class UlongExtensions
    { 
        public static long MapToLong(this ulong ulongValue)
        {
            return unchecked((long)ulongValue + long.MinValue);
        }
    }
}
