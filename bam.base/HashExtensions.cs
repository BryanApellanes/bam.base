﻿using Bam.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public static partial class HashExtensions
    {
        static Dictionary<HashAlgorithms, Func<byte[], HMAC>> _hmacs;
        static readonly object _hmacsLock = new object();
        public static Dictionary<HashAlgorithms, Func<byte[], HMAC>> Hmacs
        {
            get
            {
                return _hmacsLock.DoubleCheckLock(ref _hmacs, () => new Dictionary<HashAlgorithms, Func<byte[], HMAC>>
                {
                    {Bam.HashAlgorithms.MD5, (byte[] key) => new HMACMD5(key) },
                    {Bam.HashAlgorithms.SHA1, (byte[] key) => new HMACSHA1(key) },
                    {Bam.HashAlgorithms.SHA256, (byte[] key) => new HMACSHA256(key) },
                    {Bam.HashAlgorithms.SHA384, (byte[] key) => new HMACSHA384(key) },
                    {Bam.HashAlgorithms.SHA512, (byte[] key) => new HMACSHA512(key) }
                });
            }
        }

        static Dictionary<HashAlgorithms, Func<HashAlgorithm>> _hashAlgorithms;
        static readonly object _hashAlgorithmLock = new object();
        public static Dictionary<HashAlgorithms, Func<HashAlgorithm>> HashAlgorithms
        {
            get
            {
                return _hashAlgorithmLock.DoubleCheckLock(ref _hashAlgorithms, () => new Dictionary<HashAlgorithms, Func<HashAlgorithm>>
                {
                    { Bam.HashAlgorithms.MD5, () => MD5.Create() },
                    { Bam.HashAlgorithms.SHA1, () => SHA1.Create() },
                    { Bam.HashAlgorithms.SHA256, () => SHA256.Create() },
                    { Bam.HashAlgorithms.SHA384, () => SHA384.Create() },
                    { Bam.HashAlgorithms.SHA512, () => SHA512.Create() }
                });
            }
        }

        public static string Md5(this FileInfo file, Encoding? encoding = null)
        {
            return file.ContentHash(Bam.HashAlgorithms.MD5, encoding);
        }

        public static string Ripmd160(this FileInfo file, Encoding? encoding = null)
        {
            return file.ContentHash(Bam.HashAlgorithms.RIPEMD160, encoding);
        }

        /// <summary>
        /// Calculate the SHA1 for the contents of the specified file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Sha1(this FileInfo file, Encoding? encoding = null)
        {
            return file.ContentHash(Bam.HashAlgorithms.SHA1, encoding);
        }

        /// <summary>
        /// Calculate the SHA256 for the contents of the specified file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Sha256(this FileInfo file, Encoding? encoding = null)
        {
            return file.ContentHash(Bam.HashAlgorithms.SHA256, encoding);
        }

        public static string Sha384(this FileInfo file, Encoding? encoding = null)
        {
            return file.ContentHash(Bam.HashAlgorithms.SHA384, encoding);
        }

        public static string Sha512(this FileInfo file, Encoding? encoding = null)
        {
            return file.ContentHash(Bam.HashAlgorithms.SHA512, encoding);
        }

        public static string ContentHash(this string filePath, HashAlgorithms algorithm, Encoding? encoding = null)
        {
            return ContentHash(new FileInfo(filePath), algorithm, encoding);
        }

        /// <summary>
        /// Hashes the specified file by reading its contents as a byte array and computing the hash using the specified algorithm
        /// </summary>
        /// <param name="file"></param>
        /// <param name="algorithm"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ContentHash(this FileInfo file, HashAlgorithms algorithm, Encoding? encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            HashAlgorithm alg = HashAlgorithms[algorithm]();
            byte[] fileContents = File.ReadAllBytes(file.FullName);
            byte[] hashBytes = alg.ComputeHash(fileContents);

            return hashBytes.ToHexString();
        }

        public static string Sha1(this byte[] bytes)
        {
            return HashHexString(bytes, Bam.HashAlgorithms.SHA1);
        }

        public static string Sha256(this byte[] bytes)
        {
            return HashHexString(bytes, Bam.HashAlgorithms.SHA256);
        }

        public static string Md5(this string toBeHashed, Encoding? encoding = null)
        {
            return toBeHashed.HashHexString(Bam.HashAlgorithms.MD5, encoding);
        }

        public static string Ripmd160(this string toBeHashed, Encoding? encoding = null)
        {
            return toBeHashed.HashHexString(Bam.HashAlgorithms.RIPEMD160, encoding);
        }

        public static string Sha384(this string toBeHashed, Encoding? encoding = null)
        {
            return toBeHashed.HashHexString(Bam.HashAlgorithms.SHA384, encoding);
        }

        public static string Sha1(this string toBeHashed, Encoding? encoding = null)
        {
            return toBeHashed.HashHexString(Bam.HashAlgorithms.SHA1, encoding);
        }

        public static string Sha256(this string toBeHashed, Encoding? encoding = null)
        {
            return toBeHashed.HashHexString(Bam.HashAlgorithms.SHA256, encoding);
        }

        public static string Sha512(this string toBeHashed, Encoding? encoding = null)
        {
            return toBeHashed.HashHexString(Bam.HashAlgorithms.SHA512, encoding);
        }

        public static string HmacSha1(this string toValidate, string key, Encoding? encoding = null)
        {
            return HmacHexString(toValidate, key, Bam.HashAlgorithms.SHA1, encoding);
        }

        public static string HmacSha256Base64UrlEncoded(this string toValidate, string key, Encoding? encoding = null)
        {
            return HmacBase64UrlEncoded(toValidate, key, Bam.HashAlgorithms.SHA256, encoding);
        }
        
        public static string HmacSha256HexString(this string toValidate, string key, Encoding? encoding = null)
        {
            return HmacHexString(toValidate, key, Bam.HashAlgorithms.SHA256, encoding);
        }

        public static string HmacSha384HexString(this string toValidate, string key, Encoding? encoding = null)
        {
            return HmacHexString(toValidate, key, Bam.HashAlgorithms.SHA384, encoding);
        }

        public static string HmacSha512HexString(this string toValidate, string key, Encoding? encoding = null)
        {
            return HmacHexString(toValidate, key, Bam.HashAlgorithms.SHA512, encoding);
        }

        public static string HmacHexString(this string toValidate, string key, HashAlgorithms algorithm, Encoding? encoding = null)
        {
            byte[] bytes = HmacBytes(toValidate, key, algorithm, encoding);
            return bytes.ToHexString();
        }

        public static string HmacBase64UrlEncoded(this string toValidate, string key, HashAlgorithms algorithm, Encoding? encoding = null)
        {
            return HmacBytes(toValidate, key, algorithm, encoding).ToBase64UrlEncoded();
        }
        
        public static byte[] HmacBytes(string toValidate, string key, HashAlgorithms algorithm, Encoding encoding)
        {
            encoding = encoding ?? Encoding.UTF8;
            HMAC hmac = Hmacs[algorithm](encoding.GetBytes(key));
            byte[] bytes = hmac.ComputeHash(encoding.GetBytes(toValidate));
            return bytes;
        }

        public static string HashHexString(this string toBeHashed, HashAlgorithms algorithm, Encoding? encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(toBeHashed);

            return HashHexString(bytes, algorithm);
        }

        public static string HashHexString(this byte[] bytes, HashAlgorithms algorithm)
        {
            byte[] hashBytes = HashBytes(bytes, algorithm);

            return hashBytes.ToHexString();
        }

        public static string HashBase64UrlEncoded(byte[] bytes, HashAlgorithms algorithm)
        {
            return HashBytes(bytes, algorithm).ToBase64UrlEncoded();
        }
        
        public static byte[] HashBytes(this byte[] bytes, HashAlgorithms algorithm)
        {
            HashAlgorithm alg = HashAlgorithms[algorithm]();
            byte[] hashBytes = alg.ComputeHash(bytes);
            return hashBytes;
        }

        public static uint ToHashUint(this string toBeHashed, HashAlgorithms algorithm, Encoding? encoding = null)
        {
            byte[] hashBytes = ToHashBytes(toBeHashed, algorithm, encoding);

            return BitConverter.ToUInt32(hashBytes, 0);
        }
        
        public static int ToHashIntBetween(this string toBeHashed, HashAlgorithms algorithm, int lowerBound, int upperBound, Encoding? encoding = null)
        {
            int mod = upperBound - lowerBound;
            return (ToHashInt(toBeHashed, algorithm, encoding) % mod) + lowerBound;
        }

        public static int ToHashInt(this string toBeHashed, HashAlgorithms algorithm, Encoding? encoding = null)
        {
            byte[] hashBytes = ToHashBytes(toBeHashed, algorithm, encoding);

            return BitConverter.ToInt32(hashBytes, 0);
        }

        public static long ToHashLong(this string toBeHashed, HashAlgorithms algorithm, Encoding? encoding = null)
        {
            byte[] hashBytes = ToHashBytes(toBeHashed, algorithm, encoding);

            return BitConverter.ToInt64(hashBytes, 0);
        }

        public static ulong ToHashULong(this string toBeHashed, HashAlgorithms algorithm, Encoding? encoding = null)
        {
            byte[] hashBytes = ToHashBytes(toBeHashed, algorithm, encoding);

            return BitConverter.ToUInt64(hashBytes, 0);
        }

        public static byte[] ToHashBytes(this string toBeHashed, HashAlgorithms algorithm, Encoding? encoding = null)
        {
            HashAlgorithm alg = HashAlgorithms[algorithm]();
            encoding = encoding ?? Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(toBeHashed);
            byte[] hashBytes = alg.ComputeHash(bytes);
            return hashBytes;
        }
        public static uint ToSha1Uint(this string toBeHashed)
        {
            return ToHashUint(toBeHashed, Bam.HashAlgorithms.SHA1);
        }
        
        public static int ToSha1Int(this string toBeHashed)
        {
            return ToHashInt(toBeHashed, Bam.HashAlgorithms.SHA1);
        }

        public static int ToSha256Int(this string toBeHashed)
        {
            return ToHashInt(toBeHashed, Bam.HashAlgorithms.SHA256);
        }

        public static long ToSha256Long(this string toBeHashed)
        {
            return ToHashLong(toBeHashed, Bam.HashAlgorithms.SHA256);
        }

        public static ulong ToSha256ULong(this string toBeHashed)
        {
            return ToHashULong(toBeHashed, Bam.HashAlgorithms.SHA256);
        }

        public static long ToSha512Long(this string toBeHashed)
        {
            return ToHashLong(toBeHashed, Bam.HashAlgorithms.SHA512);
        }

        public static ulong ToSha512ULong(this string toBeHashed)
        {
            return ToHashULong(toBeHashed, Bam.HashAlgorithms.SHA512);
        }

        public static long ToSha1Long(this string toBeHashed)
        {
            return ToHashLong(toBeHashed, Bam.HashAlgorithms.SHA1);
        }

    }
}
