using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net
{
    public static class GenericExtensions
    {
        public static bool In<T>(this T obj, IEnumerable<T> options)
        {
            return new List<T>(options).Contains(obj);
        }

        public static bool In<T>(this T obj, params T[] options)
        {
            return new List<T>(options).Contains(obj);
        }
    }
}
