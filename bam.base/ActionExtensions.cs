using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net
{
    public static class ActionExtensions
    {
        public static bool Try(this Action toTry)
        {
            return Try(toTry, out Exception ignore);

        }

        public static bool Try(this Action toTry, out Exception ex)
        {
            ex = null;
            try
            {
                toTry();
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }
    }
}
