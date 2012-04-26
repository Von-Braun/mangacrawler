using System;
using System.IO;

namespace TomanuExtensions.Utils
{
    public static class Hash
    {
        public static string CalculateSHA1ForFile(string a_file)
        {
            if (!FileUtils.IsFilePathValid(a_file))
                return null;

            if (!new FileInfo(a_file).Exists)
                return null;

            using (var stream = File.OpenRead(a_file))
                return CalculateSHA1(stream);
        }

        public static string CalculateSHA1(Stream a_stream)
        {
            using (var hasher = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hash = hasher.ComputeHash(a_stream);
                return ConvertBytesToHexString(hash);
            }
        }

        public static string CalculateSHA1(byte[] a_data)
        {
            using (var hasher = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hash = hasher.ComputeHash(a_data);
                return ConvertBytesToHexString(hash);
            }
        }

        private static string ConvertBytesToHexString(byte[] a_in)
        {
            return BitConverter.ToString(a_in).ToUpper().Replace("-", "");
        }

        public static string CalculateSHA256(Stream a_stream)
        {
            using (System.Security.Cryptography.SHA256Cng sha256 = new System.Security.Cryptography.SHA256Cng())
            {
                byte[] hash = sha256.ComputeHash(a_stream);
                return ConvertBytesToHexString(hash);
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