using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Net
{
    public interface IValueTransformer<TUntransformed, TTransformed>
    {
        IValueReverseTransformer<TTransformed, TUntransformed> GetReverseTransformer();

        /// <summary>
        /// When implemented in a derived class, transforms the specified value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        TTransformed Transform(TUntransformed value);
    }
}
