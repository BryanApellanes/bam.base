using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net
{
    public static class CharExtensions
    {
        public static bool IsNumber(this char c)
        {
            int val = Convert.ToInt32(c);
            return (val > 47 && val < 58);
        }
        public static bool IsLetter(this char c)
        {
            int val = Convert.ToInt32(c);
            return (val > 96 && val < 123) || (val > 64 && val < 91);
        }
    }
}
