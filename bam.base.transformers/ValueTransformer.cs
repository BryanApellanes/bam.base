//using Bam;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bam
{
    public abstract class ValueTransformer<TInput, TOutput> : IValueTransformer<TInput, TOutput>, IValueConverter<TOutput>
    {
        public ValueTransformer()
        {
            this.ValueConverter = new JsonValueConverter<TOutput>();
        }

        public abstract TOutput Transform(TInput input);

        public abstract TInput ReverseTransform(TOutput output);

        public abstract IValueReverseTransformer<TOutput, TInput> GetReverseTransformer();

        public IValueConverter<TOutput> ValueConverter { get; set; }

        public TOutput ConvertBytesToObject(byte[] bytes)
        {
            return ValueConverter.ConvertBytesToObject(bytes);
        }

        public byte[] ConvertObjectToBytes(TOutput value)
        {
            return ValueConverter.ConvertObjectToBytes(value);
        }

        public string ConvertBytesToString(byte[] bytes)
        {
            return ValueConverter.ConvertBytesToString(bytes);
        }

        public byte[] ConvertStringToBytes(string value)
        {
            return ValueConverter.ConvertStringToBytes(value);
        }

    }
}
