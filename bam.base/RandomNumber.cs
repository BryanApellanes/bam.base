/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam
{
    public abstract class RandomNumber
    {
        public static int UpTo(int max = 100)
        {
            return RandomHelper.Next(max);
        }

        public static int Between(int min, int max)
        {
            return RandomHelper.Next(min, max);
        }
    }
}
