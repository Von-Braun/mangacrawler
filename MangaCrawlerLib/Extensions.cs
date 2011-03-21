using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MangaCrawlerLib
{
    internal static class Extensions
    {
        public static String RemoveFromRight(this string a_str, int a_chars)
        {
            return a_str.Remove(a_str.Length - a_chars);
        }

        public static String RemoveFromLeft(this string a_str, int a_chars)
        {
            return a_str.Remove(0, a_chars);
        }

        public static String Left(this string a_str, int a_count)
        {
            return a_str.Substring(0, a_count);
        }

        public static String Right(this string a_str, int a_count)
        {
            return a_str.Substring(a_str.Length-a_count, a_count);
        }

        public static void RemoveLast<T>(this List<T> a_list)
        {
            a_list.Remove(a_list.Last());
        }

        public static void DeleteAll(this DirectoryInfo a_dir_info)
        {
            if (!a_dir_info.Exists)
                return;

            foreach (FileInfo file_info in a_dir_info.GetFiles())
                file_info.Delete();

            foreach (DirectoryInfo dir_info in a_dir_info.GetDirectories())
                dir_info.DeleteAll();

            a_dir_info.Delete(false);
        }
    }
}
