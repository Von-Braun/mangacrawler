using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MangaCrawlerLib
{
    internal static class FileUtils
    {
        public static string RemoveInvalidFileDirectoryCharacters(string a_path)
        {
            foreach (char c in Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct())
                a_path = a_path.Replace(new String(new char[] { c }), "");
            return a_path;
        }
    }
}
