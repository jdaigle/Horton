using System;
using System.Security.Cryptography;
using System.Text;

namespace SqlPatch {
    public static class HashExtensions {

        private static MD5 hasher = MD5.Create();

        public static Guid MD5Hash(this string value) {
            var buffer = UTF8Encoding.UTF8.GetBytes(value);
            return MD5Hash(buffer);
        }

        public static Guid MD5Hash(this byte[] value) {
            var buffer = hasher.ComputeHash(value);
            return new Guid(buffer);
        }
    }
}
