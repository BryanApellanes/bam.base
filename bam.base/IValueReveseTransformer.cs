using System;
using System.Collections.Generic;
using System.Text;

namespace Bam
{
    public interface IValueReverseTransformer<TTransformed, TReversed>
    {
        IValueTransformer<TReversed, TTransformed> GetTransformer();

        TReversed ReverseTransform(TTransformed transformed);
    }
}
