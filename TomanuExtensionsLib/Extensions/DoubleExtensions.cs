using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using System.Globalization;

namespace TomanuExtensions
{
    [DebuggerStepThrough]
    public static class DoubleExtensions
    {
        public static bool IsNumber(this double a_d)
        {
            return !Double.IsInfinity(a_d) && !Double.IsNaN(a_d);
        }

        public static double Fraction(this double a_d)
        {
            return a_d - Math.Truncate(a_d);
        }

        public static int Round(this double a_d)
        {
            if (a_d >= 0)
            {
                double d1 = a_d + 0.5;

                if (d1 == (double)((int)d1))
                    return (int)(d1 - 1);
                else
                    return (int)d1;
            }
            else
            {
                double d1 = a_d - 0.5;

                if (d1 == (double)((int)d1))
                    return (int)(d1 + 1);
                else
                    return (int)d1;
            }
        }

        public static bool IsOne(this double a_d, double a_precision)
        {
            return Math.Abs(a_d - 1) < a_precision;
        }

        public static bool IsZero(this double a_d, double a_precision)
        {
            return Math.Abs(a_d) < a_precision;
        }

        public static bool AlmostEqual(this double a_d1, double a_d2, double a_precision)
        {
            double ad1 = Math.Abs(a_d1);
            double ad2 = Math.Abs(a_d2);

            if (ad1 < a_precision)
                a_d1 = 0;
            if (ad2 < a_precision)
                a_d2 = 0;

            if (a_d1 == a_d2)
                return true;

            return (Math.Abs(a_d1 - a_d2) / Math.Max(ad1, ad2)) < (a_precision * 4);
        }

        // Extension method may cause problem: -d.Limit(a, b) means
        // -(d.Limit(a,b))
        public static double Limit(double a_d, double a_min_inclusive, 
            double a_max_inclusive)
        {
            Debug.Assert(a_min_inclusive <= a_max_inclusive);

            if (a_d < a_min_inclusive)
                return a_min_inclusive;
            else if (a_d > a_max_inclusive)
                return a_max_inclusive;
            else
                return a_d;
        }

        // Extension method may cause problem: -d.InRange(a, b) means
        // -(d.InRange(a,b))
        public static bool InRange(double a_d, double a_min_inclusive,
            double a_max_inclusive)
        {
            Debug.Assert(a_min_inclusive <= a_max_inclusive);

            return (a_d >= a_min_inclusive) && (a_d <= a_max_inclusive);
        }
    }
}
