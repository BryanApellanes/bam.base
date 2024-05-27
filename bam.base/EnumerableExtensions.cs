using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public static class EnumerableExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task[] EachAsync<T>(this IEnumerable<T> arr, Action<T> action)
        {
            List<Task> results = new List<Task>();
            foreach (T item in arr)
            {
                results.Add(Task.Run(() => action(item)));
            };
            return results.ToArray();
        }

        /// <summary>
        /// Iterate over the current IEnumerable using `foreach`, passing
        /// each element to the specified action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void Each<T>(this IEnumerable<T> arr, Action<T> action)
        {
            foreach (T item in arr)
            {
                action(item);
            }
        }

        /// <summary>
        /// Iterate over the current array passing
        /// each element to the specified action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        /*public static void Each<T>(this T[] arr, Action<T> action)
        {
            if (arr != null)
            {
                int l = arr.Length;
                for (int i = 0; i < l; i++)
                {
                    action(arr[i]);
                }
            }
        }*/

        /// <summary>
        /// Iterate over the current IEnumerable passing
        /// each element to the specified function, if the specified function returns false the remainder of the
        /// iteration is stopped. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="function"></param>
        public static void Each<T>(this IEnumerable<T> enumerable, Func<T, bool> function)
        {
            foreach (T item in enumerable)
            {
                if (!function(item))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Iterate over the current array passing
        /// each element to the specified function.  
        /// Return true to continue the loop return 
        /// false to stop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="function"></param>
        public static void Each<T>(this T[] arr, Func<T, bool> function)
        {
            if (arr != null)
            {
                int l = arr.Length;
                for (int i = 0; i < l; i++)
                {
                    if (!function(arr[i]))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Iterate over the current IEnumerable passing
        /// each element to the specified function.  
        /// Return true to continue the loop return 
        /// false to stop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="function"></param>
        public static void Each<T>(this IEnumerable<T> enumerable, Func<T, int, bool> function)
        {
            int counter = 0;
            foreach (T item in enumerable)
            {
                if (!function(item, counter))
                {
                    break;
                }

                counter++;
            }
        }

        /// <summary>
        /// Iterate over the current array passing
        /// each element to the specified function.  
        /// Return true to continue the loop return 
        /// false to stop
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="function"></param>
        public static void Each<T>(this T[] arr, Func<T, int, bool> function)
        {
            if (arr != null)
            {
                int l = arr.Length;
                for (int i = 0; i < l; i++)
                {
                    if (!function(arr[i], i))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Iterate over the current IEnumerable passing
        /// each element to the specified action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        public static void Each<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            int counter = 0;
            foreach (T item in enumerable)
            {
                action(item, counter);
                counter++;
            }
        }

        /// <summary>
        /// Iterate over the current array passing
        /// each element to the specified action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void Each<T>(this T[] arr, Action<T, int> action)
        {
            if (arr != null)
            {
                int l = arr.Length;
                for (int i = 0; i < l; i++)
                {
                    action(arr[i], i);
                }
            }
        }

        public static void Each<T>(this IEnumerable<T> enumerable, dynamic context, Action<dynamic, T> action)
        {
            foreach (T item in enumerable)
            {
                action(context, item);
            }
        }

        /// <summary>
        /// Iterate over the current IEnumerable 
        /// from the specified index passing
        /// each element to the specified action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void Rest<T>(this IEnumerable<T> arr, int startIndex, Action<T, int> action)
        {
            arr.ToArray().Rest(startIndex, action);
        }

        /// <summary>
        /// Iterate over the current array from the 
        /// specified index passing
        /// each element to the specified action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void Rest<T>(this T[] arr, int startIndex, Action<T, int> action)
        {
            if (arr != null)
            {
                int l = arr.Length;
                for (int i = startIndex; i < l; i++)
                {
                    action(arr[i], i);
                }
            }
        }

        /// <summary>
        /// Iterate over the current IEnumerable starting from the specified index
        /// passing each element to the specified action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void Rest<T>(this IEnumerable<T> arr, int startIndex, Action<T> action)
        {
            arr.ToArray().Rest(startIndex, action);
        }

        /// <summary>
        /// Iterate over the current array from the specified
        /// startIndex passing
        /// each element to the specified action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void Rest<T>(this T[] arr, int startIndex, Action<T> action)
        {
            if (arr != null && startIndex <= arr.Length - 1)
            {
                int l = arr.Length;
                for (int i = startIndex; i < l; i++)
                {
                    action(arr[i]);
                }
            }
        }

        /// <summary>
        /// Iterate backwards over the specified array (IEnumerable).
        /// Allows one to remove the current element of each iteration, 
        /// if necessary, without causing an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void BackwardsEach<T>(this IEnumerable<T> arr, Action<T> action)
        {
            arr.ToArray().BackwardsEach(action);
        }

        /// <summary>
        /// Iterate backwards over the specified array (IEnumerable).
        /// Allows one to remove the current element of each iteration, 
        /// if necessary, without causing an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void BackwardsEach<T>(this T[] arr, Action<T> action)
        {
            if (arr != null)
            {
                int l = arr.Length;
                for (int i = l - 1; i >= 0; i--)
                {
                    action(arr[i]);
                }
            }
        }

        /// <summary>
        /// Iterate backwards over the specified array (IEnumerable).
        /// Allows one to remove the current element of each iteration, 
        /// if necessary, without causing an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void BackwardsEach<T>(this IEnumerable<T> arr, Func<T, bool> function)
        {
            arr.ToArray().BackwardsEach(function);
        }

        /// <summary>
        /// Iterate backwards over the specified array (IEnumerable).
        /// Allows one to remove the current element of each iteration, 
        /// if necessary, without causing an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void BackwardsEach<T>(this T[] arr, Func<T, bool> function)
        {
            if (arr != null)
            {
                int l = arr.Length;
                for (int i = l - 1; i >= 0; i--)
                {
                    bool result = function(arr[i]);
                    if (!result)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Iterate backwards over the specified array (IEnumerable).
        /// Allows one to remove the current element of each iteration, 
        /// if necessary, without causing an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        public static void BackwardsEach<T>(this IEnumerable<T> arr, Func<T, int, bool> function)
        {
            arr.ToArray().BackwardsEach(function);
        }

        /// <summary>
        /// Iterate backwards over the specified array (IEnumerable).
        /// Allows one to remove the current element of each iteration, 
        /// if necessary, without causing an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        public static void BackwardsEach<T>(this T[] arr, Func<T, int, bool> function)
        {
            if (arr != null)
            {
                int l = arr.Length;
                for (int i = l - 1; i >= 0; i--)
                {
                    bool result = function(arr[i], i);
                    if (!result)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Iterate backwards over the specified array (IEnumerable).
        /// This will allow one to remove the current element without
        /// causing an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void BackwardsEach<T>(this IEnumerable<T> arr, Action<T, int> action)
        {
            arr.ToArray().BackwardsEach(action);
        }

        /// <summary>
        /// Iterate backwards over the specified array (IEnumerable).
        /// This will allow one to remove the current element without
        /// causing an exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="action"></param>
        public static void BackwardsEach<T>(this T[] arr, Action<T, int> action)
        {
            if (arr != null)
            {
                int l = arr.Length;
                for (int i = l - 1; i >= 0; i--)
                {
                    action(arr[i], i);
                }
            }
        }

        /// <summary>
        /// Iterate over the current array passing 
        /// each element to the specified function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="func"></param>
        /// <returns>The result of each call to the specified function</returns>
        public static T[] Each<T>(this object[] arr, Func<object, T> func)
        {
            int l = arr.Length;
            T[] result = new T[l];
            for (int i = 0; i < l; i++)
            {
                result[i] = func(arr[i]);
            }

            return result;
        }

    }
}
