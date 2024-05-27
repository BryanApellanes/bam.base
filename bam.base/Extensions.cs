using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    internal class Extensions
    {
        /// <summary>
        /// Get a random boolean
        /// </summary>
        /// <returns></returns>
        public static bool RandomBool()
        {
            return RandomHelper.Next(2) == 1;
        }
    }
}
