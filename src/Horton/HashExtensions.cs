using System;
using System.Security.Cryptography;
using System.Text;

namespace Horton
{
    public static class HashExtensions
    {
        private static SHA1 sha1 = SHA1.Create();
        private static MD5 md5 = MD5.Create();

        public static string SHA1Hash(this string value)
        {
            var buffer = Encoding.UTF8.GetBytes(value);
            return SHA1Hash(buffer);
        }

        public static string SHA1Hash(this byte[] value)
        {
            var buffer = sha1.ComputeHash(value);
            return HexStringFromBytes(buffer);
        }

        public static Guid MD5Hash(this string value)
        {
            var buffer = Encoding.UTF8.GetBytes(value);
            return MD5Hash(buffer);
        }

        public static Guid MD5Hash(this byte[] value)
        {
            var buffer = md5.ComputeHash(value);
            return new Guid(buffer);
        }

        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
    }
}
