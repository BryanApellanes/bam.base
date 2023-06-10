using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Bam.Net
{
    public class ByteTransformerPipeline : IValueTransformer<byte[], byte[]>
    {
        readonly List<IValueTransformer<byte[], byte[]>> _transformers;

        public ByteTransformerPipeline()
        {
            this._transformers = new List<IValueTransformer<byte[], byte[]>>();
        }

        protected internal IList<IValueTransformer<byte[], byte[]>> Transformers
        {
            get => _transformers;
        }

        public void Add(IValueTransformer<byte[], byte[]> transformer)
        {
            _transformers.Add(transformer);
        }

        public void Remove(IValueTransformer<byte[], byte[]> transformer)
        {
            _transformers.Remove(transformer);
        }

        public bool Contains(IValueTransformer<byte[], byte[]> transformer)
        {
            return _transformers.Contains(transformer);
        }

        public IValueReverseTransformer<byte[], byte[]> GetReverseTransformer()
        {
            return ByteReverseTransformerPipeline.For(this);
        }

        public byte[] Transform(byte[] value)
        {
            byte[] result = value;
            foreach(IValueTransformer<byte[], byte[]> transformer in _transformers)
            {
                result = transformer.Transform(result);
            }
            return result;
        }
    }
}
