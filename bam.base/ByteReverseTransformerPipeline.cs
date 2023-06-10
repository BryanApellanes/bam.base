using System.Collections.Generic;

namespace Bam.Net
{
    public class ByteReverseTransformerPipeline : IValueReverseTransformer<byte[], byte[]>
    {
        readonly List<IValueReverseTransformer<byte[], byte[]>> _reverseTransformers;

        public ByteReverseTransformerPipeline()
        {
            this._reverseTransformers = new List<IValueReverseTransformer<byte[], byte[]>>();
        }

        public void Add(IValueReverseTransformer<byte[], byte[]> transformer)
        {
            _reverseTransformers.Add(transformer);
        }

        public void Remove(IValueReverseTransformer<byte[], byte[]> transformer)
        {
            _reverseTransformers.Remove(transformer);
        }

        public bool Contains(IValueReverseTransformer<byte[], byte[]> transformer)
        {
            return _reverseTransformers.Contains(transformer);
        }

        public IValueTransformer<byte[], byte[]> GetTransformer()
        {
            ByteTransformerPipeline byteTransformerPipeline = new ByteTransformerPipeline();
            _reverseTransformers.BackwardsEach(reverseTransformer => byteTransformerPipeline.Add(reverseTransformer.GetTransformer()));
            return byteTransformerPipeline;
        }

        public byte[] ReverseTransform(byte[] transformed)
        {
            byte[] result = transformed;
            foreach(IValueReverseTransformer<byte[], byte[]> reverseTransformer in _reverseTransformers)
            {
                result = reverseTransformer.ReverseTransform(result);
            }
            return result;
        }

        public static ByteReverseTransformerPipeline For(ByteTransformerPipeline transformerPipeline)
        {
            ByteReverseTransformerPipeline reverseTransformerPipeline = new ByteReverseTransformerPipeline();
            transformerPipeline.Transformers.BackwardsEach(transformer => reverseTransformerPipeline.Add(transformer.GetReverseTransformer()));
            return reverseTransformerPipeline;
        }
    }
}
