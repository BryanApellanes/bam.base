using System;
using System.Collections.Generic;
using System.Text;

namespace Bam
{
    public class JsonValueConverter<T> : IValueConverter<T>
    {
        public JsonValueConverter()
        {
            this.Encoding = Encoding.UTF8;
        }

        public Encoding Encoding { get; set; }

        public T ConvertBytesToObject(byte[] bytes)
        {
            string json = ConvertBytesToString(bytes);
            return json.FromJson<T>();
        }

        public string ConvertBytesToString(byte[] bytes)
        {
            return Encoding.GetString(bytes);
        }

        public byte[] ConvertObjectToBytes(T value)
        {
            string json = value.ToJson();
            return ConvertStringToBytes(json);
        }

        public byte[] ConvertStringToBytes(string value)
        {
            return Encoding.GetBytes(value);
        }
    }
}
