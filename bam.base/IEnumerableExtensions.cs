using System.Collections;

namespace Bam
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> CopyAs<T>(this IEnumerable enumerable) where T : new()
        {
            foreach (object o in enumerable)
            {
                yield return o.CopyAs<T>();
            }
        }

        public static IEnumerable<object> CopyAs(this IEnumerable enumerable, Type type, params object[] ctorParams)
        {
            foreach (object o in enumerable)
            {
                yield return o.CopyAs(type, ctorParams);
            }
        }
    }
}
