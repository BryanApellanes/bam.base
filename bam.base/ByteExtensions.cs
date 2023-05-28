using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net
{
    public static class ByteExtensions
    {
        public static string FromBytes(this byte[] text, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return encoding.GetString(text);
        }

        public static string ToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public static string ToBase64UrlEncoded(this byte[] data)
        {
            return WebEncoders.Base64UrlEncode(data);
        }

        public static string ToBase64(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static IObjectDecoder ObjectDecoder
        {
            get;
            set;
        }

        public static object Decode(this byte[] data, Type type)
        {
            Args.ThrowIfNull(ObjectDecoder, $"{nameof(ByteExtensions)}.{nameof(ObjectDecoder)}");
            return ObjectDecoder.Decode(data, type);
        }

        public static T Decode<T>(this byte[] data)
        {
            Args.ThrowIfNull(ObjectDecoder, $"{nameof(ByteExtensions)}.{nameof(ObjectDecoder)}");
            return ObjectDecoder.Decode<T>(data);
        }
    }
}
