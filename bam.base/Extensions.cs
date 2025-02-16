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
