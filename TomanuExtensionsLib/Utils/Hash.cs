using System;
using System.IO;

namespace TomanuExtensions.Utils
{
    public static class Hash
    {
        public static string CalculateSHA256(Stream a_stream)
        {
            using (System.Security.Cryptography.SHA256Cng sha256 = new System.Security.Cryptography.SHA256Cng())
            {
                byte[] hash = sha256.ComputeHash(a_stream);
                return BitConverter.ToString(hash).ToUpper();
            }
        }

        public static void CalculateSHA256(Stream a_stream, out byte[] a_hash)
        {
            using (System.Security.Cryptography.SHA256Cng sha256 = new System.Security.Cryptography.SHA256Cng())
            {
                a_hash = sha256.ComputeHash(a_stream);
            }
        }
    }
}