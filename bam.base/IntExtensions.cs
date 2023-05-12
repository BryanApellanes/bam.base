using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net
{
    public static class IntExtensions
    {
        /// <summary>
        /// Execute the specified Func this 
        /// many times
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T[] Times<T>(this int count, Func<int, T> func)
        {
            T[] results = new T[count];
            for (int i = 0; i < count; i++)
            {
                results[i] = func(i);
            }

            return results;
        }

        /// <summary>
        /// Return the specified number of random letters
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string RandomLetters(this int count)
        {
            return count.RandomString();
        }
        /// <summary>
        /// Return a random string of the specified
        /// length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(this int length)
        {
            return RandomString(length, true, true);
        }
        public static string RandomString(this int length, bool mixCase, bool includeNumbers)
        {
            if (length <= 0)
                throw new InvalidOperationException("length must be greater than 0");


            string retTemp = string.Empty;

            for (int i = 0; i < length; i++)
            {
                if (includeNumbers)
                    retTemp += RandomChar().ToString();
                else
                    retTemp += StringExtensions.RandomLetter();
            }

            if (mixCase)
            {
                string upperIzed = StringExtensions.MixCase(retTemp);

                retTemp = upperIzed;
            }

            return retTemp;
        }

        /// <summary>
        /// Returns a random lower-case character a-z or 0-9
        /// </summary>
        /// <returns>String</returns>
        public static char RandomChar()
        {
            if (Extensions.RandomBool())
            {
                return StringExtensions.RandomLetter().ToCharArray()[0];
            }
            else
            {
                return RandomNumber().ToString().ToCharArray()[0];
            }
        }

        /// <summary>
        /// Returns a pseudo-random number from 0 to 9.
        /// </summary>
        /// <returns></returns>
        public static int RandomNumber()
        {
            return RandomNumber(10);
        }

        public static int RandomNumber(int max)
        {
            return RandomHelper.Next(max);
        }
    }
}
