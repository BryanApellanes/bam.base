using System;
using System.Collections.Generic;
using System.Text;
using Bam.Base.Transformers;

namespace Bam
{
    public class BsonReverseTransformer<TData> : IValueReverseTransformer<byte[], TData>
    {
        public TData ReverseTransform(byte[] encoded)
        {
            return encoded.FromBson<TData>();
        }

        public IValueTransformer<TData, byte[]> GetTransformer()
        {
            return new BsonTransformer<TData>();
        }
    }
}