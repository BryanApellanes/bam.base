using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
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

        public static T Largest<T>(this T[] values)
        {
            if (values.Length == 0)
            {
                return default(T);
            }

            T result = values[0];
            values.Each(s => result = s.ToString().CompareTo(result.ToString()) == 1 ? s : result);
            return result;
        }
    }
}
