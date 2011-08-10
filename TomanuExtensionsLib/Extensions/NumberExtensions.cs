using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomanuExtensions
{
    public static class Range
    {
        public static bool InRange(this float a_value, float a_inclusiveMin, float a_inclusiveMax)
        {
            return (a_value >= a_inclusiveMin) && (a_value <= a_inclusiveMax);
        }

        public static bool InRange(this int a_value, int a_inclusiveMin, int a_inclusiveMax)
        {
            return (a_value >= a_inclusiveMin) && (a_value <= a_inclusiveMax);
        }

        public static bool InRange(this uint a_value, uint a_inclusiveMin, uint a_inclusiveMax)
        {
            return (a_value >= a_inclusiveMin) && (a_value <= a_inclusiveMax);
        }
    }
}
