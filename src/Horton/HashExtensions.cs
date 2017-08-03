using System.Security.Cryptography;
using System.Text;

namespace Horton
{
    public static class HashExtensions
    {
        private static SHA1 sha1 = SHA1.Create();

        public static byte[] SHA1Hash(this string value) => SHA1Hash(Encoding.UTF8.GetBytes(value));

        public static byte[] SHA1Hash(this byte[] value) => sha1.ComputeHash(value);

        public static bool HashMatches(this byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
