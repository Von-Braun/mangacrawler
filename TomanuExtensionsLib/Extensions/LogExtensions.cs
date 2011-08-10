using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TomanuExtensions
{
    [DebuggerStepThrough]
    public static class LogUtils
    {
        public static void Log(this object v)
        {
            System.Console.WriteLine(v);
        }

        public static void Log(this string v)
        {
            System.Console.WriteLine(v);
        }

        public static void Log(this object[] v)
        {
            v.ForEachWithIndex(
                delegate(object s, int index) { System.Console.WriteLine("{0}>{1}", index, s); } );
        }

        public static void Log<T>(this IEnumerable<T> v)
        {
            v.ForEachWithIndex<T>(
                 delegate(T s, int index) { System.Console.WriteLine("{0}>{1}", index, s); });
        }
    }
}
