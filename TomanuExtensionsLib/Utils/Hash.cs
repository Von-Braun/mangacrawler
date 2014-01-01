using System;
using System.IO;

namespace TomanuExtensions.Utils
{
    public static class Hash
    {
        private static string ConvertBytesToHexString(byte[] a_in, bool a_group = true)
        {
            string hex = BitConverter.ToString(a_in).ToUpper();

            if (a_group)
            {
                string[] ar = BitConverter.ToString(a_in).ToUpper().Split(new char[] { '-' });

                hex = "";

                for (int i = 0; i < ar.Length / 4; i++)
                {
                    if (i != 0)
                        hex += "-";
                    hex += ar[i * 4] + ar[i * 4 + 1] + ar[i * 4 + 2] + ar[i * 4 + 3];
                }
            }
            else
                hex = hex.Replace("-", "");

            return hex;
        }

        public static string CalculateSHA256(Stream a_stream)
        {
            using (System.Security.Cryptography.SHA256Cng sha256 = new System.Security.Cryptography.SHA256Cng())
            {
                byte[] hash = sha256.ComputeHash(a_stream);
                return ConvertBytesToHexString(hash, true);
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