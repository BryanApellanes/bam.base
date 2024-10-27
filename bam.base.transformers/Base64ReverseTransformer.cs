using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Base.Transformers
{
    public class Base64ReverseTransformer : IValueReverseTransformer<string, byte[]>
    {
        public byte[] ReverseTransform(string encoded)
        {
            return encoded.FromBase64();
        }

        public IValueTransformer<byte[], string> GetTransformer()
        {
            return new Base64Transformer();
        }
    }
}