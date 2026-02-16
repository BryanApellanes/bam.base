namespace Bam
{
    public static class LongExtensions
    { 
public static long Largest(this long[] longs)
        {
            if (longs.Length == 0)
            {
                return -1;
            }

            long largest = longs[0];
            longs.Each(l => largest = l > largest ? l : largest);
            return largest;
        }
    }
}
