namespace Bam
{
    public interface IValueReverseTransformer<TTransformed, TReversed>
    {
        IValueTransformer<TReversed, TTransformed> GetTransformer();

        TReversed? ReverseTransform(TTransformed transformed);
    }
}
