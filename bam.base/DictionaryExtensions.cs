using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds the specified value if the specified key has not been added, returns true if the key had not already been added, false if the value
        /// is not added because the key already exists.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>true if the value was added because no value existed, false if a value with the same key is already in the dictionary.</returns>
        public static bool AddMissing<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        {
            if (dictionary.TryAdd(key, value))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Set the value for the specified key in the dictionary in a way that won't 
        /// throw an exception if the key isn't already there.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        {
            if (!AddMissing(dictionary, key, value))
            {
                dictionary[key] = value;
            }
        }
        public static object ToInstance(this Dictionary<object, object> dictionary, Type type, params object[] ctorParams)
        {
            return CopyAs(dictionary, type, ctorParams);
        }

        public static object CopyAs(this Dictionary<object, object> dictionary, Type type, params object[] ctorParams)
        {
            object result = type.Construct(ctorParams);
            foreach (object key in dictionary.Keys)
            {
                result.Property(key.ToString(), dictionary[key]);
            }

            return result;
        }
    }
}
