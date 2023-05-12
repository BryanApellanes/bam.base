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

        public static long Largest(this long[] longs)
        {
            if (longs.Length == 0)
            {
                return -1;
            }

            long largest = longs[0];
            longs.Each(l => largest = l > largest ? l : largest);
            return largest;
        }
    }
}
