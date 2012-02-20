using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    public static class IDGenerator
    {
        private static Object s_lock = new Object();
        private static int s_id = 0;

        public static int Next()
        {
            lock (s_lock)
            {
                s_id++;
                return s_id;
            }
        }
    }
}
