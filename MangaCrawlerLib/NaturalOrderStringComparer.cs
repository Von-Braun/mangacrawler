using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MangaCrawlerLib
{
    internal class NaturalOrderStringComparer : IComparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(String a_x, String a_y);

        public int Compare(string a_x, string a_y)
        {
            return StrCmpLogicalW(a_x, a_y);
        }
    }
}
