namespace Bam
{
    public static class ActionExtensions
    {
        public static bool Throws(this Action throws)
        {
            return throws.Try() == false;
        }

        public static bool Throws(this Action throws, out Exception? exception)
        {
            return throws.Try(out exception) == false;
        }
        
        public static bool Try(this Action toTry)
        {
            return Try(toTry, out _);

        }

        public static bool Try(this Action toTry, out Exception? ex)
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
