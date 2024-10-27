using System;
using System.Text;

namespace Bam
{
    public class ValueReverseTransformerPipeline<TData> : IValueReverseTransformer<byte[], TData>
    {
        public ValueReverseTransformerPipeline(ValueTransformerPipeline<TData> tranformerPipeline)
        {
            this.TransformerPipeline = tranformerPipeline;
        }

        public ValueTransformerPipeline<TData> TransformerPipeline { get; set; }

        public IValueTransformer<TData, byte[]> GetTransformer()
        {
            return this.TransformerPipeline;
        }

        public TData ReverseTransform(byte[] transformed)
        {
            IValueReverseTransformer<byte[], byte[]> reverseTransformer = TransformerPipeline.ByteTransformerPipeline.GetReverseTransformer();
            byte[] utf8 = reverseTransformer.ReverseTransform(transformed);
            string reversedString = Encoding.UTF8.GetString(utf8);

            return ConvertStringToData(reversedString);
        }

        public virtual TData ConvertStringToData(string stringValue)
        {
            return stringValue.FromJson<TData>();
        }
    }
}
