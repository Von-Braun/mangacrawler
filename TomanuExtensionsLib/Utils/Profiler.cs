using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TomanuExtensions
{
    public class Profiler
    {
        public static long Profile(Action a_action, int a_times)
        {
            long result = long.MaxValue;
            Stopwatch sw = new Stopwatch();

            for (int i = 0; i < a_times; i++)
            {
                sw.Restart();

                a_action();

                sw.Stop();

                if (sw.ElapsedMilliseconds < result)
                    result = sw.ElapsedMilliseconds;
            }

            return result;
        }
    }
}
