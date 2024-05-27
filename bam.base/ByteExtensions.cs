using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
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

        public static byte[] HexToByteArray(this string hex)
        {
            return HashToByteArray(hex);
        }
        
        public static byte[] HashToByteArray(this string hex) {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
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
