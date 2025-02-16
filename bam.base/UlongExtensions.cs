namespace Bam
{
    public static class UlongExtensions
    { 
        public static long MapToLong(this ulong ulongValue)
        {
            return unchecked((long)ulongValue + long.MinValue);
        }
    }
}
