using System;
using System.Collections.Generic;
using System.Text;

namespace Bam
{
    public class ValueTransformerPipeline<TData> : IValueTransformer<TData, byte[]>
    {
        public ValueTransformerPipeline()
        {
            this.ByteTransformerPipeline = new ByteTransformerPipeline();
        }

        protected internal ByteTransformerPipeline ByteTransformerPipeline { get; set; }

        public void Add(IValueTransformer<byte[], byte[]> transformer)
        {
            ByteTransformerPipeline.Add(transformer);
        }

        public void Remove(IValueTransformer<byte[], byte[]> transformer)
        {
            ByteTransformerPipeline.Remove(transformer);
        }

        public bool Contains(IValueTransformer<byte[], byte[]> transformer)
        {
            return ByteTransformerPipeline.Contains(transformer);
        }

        public virtual IValueReverseTransformer<byte[], TData> GetReverseTransformer()
        {
            return new ValueReverseTransformerPipeline<TData>(this);
        }

        public byte[] Transform(TData value)
        {
            string json = ConvertDataToString(value);
            byte[] utf8 = Encoding.UTF8.GetBytes(json);

            return ByteTransformerPipeline.Transform(utf8);
        }

        public virtual string ConvertDataToString(TData value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return value.ToJson();
        }
    }
}
