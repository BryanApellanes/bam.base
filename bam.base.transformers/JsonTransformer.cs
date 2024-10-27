using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Base.Transformers
{
    public class JsonTransformer<TInput> : ValueTransformer<TInput, string>
    {
        public JsonTransformer() 
        {
        }

        public override TInput ReverseTransform(string output)
        {
            return GetReverseTransformer().ReverseTransform(output);
        }

        public override string Transform(TInput value)
        {
            return value.ToJson();
        }

        public override IValueReverseTransformer<string, TInput> GetReverseTransformer()
        {
            return new JsonReverseTransformer<TInput>();
        }
    }
}