using System.Diagnostics;

namespace TomanuExtensions.Utils
{
    public static class Bits
    {
        public static bool IsSet(byte a_byte, int a_bitIndex)
        {
            Debug.Assert(a_bitIndex >= 0);
            Debug.Assert(a_bitIndex <= 7);

            return (a_byte & (1 << a_bitIndex)) != 0;
        }

        public static void SetBit(ref byte a_byte, int a_bitIndex, bool a_bitValue)
        {
            Debug.Assert(a_bitIndex >= 0);
            Debug.Assert(a_bitIndex <= 7);

            if (a_bitValue)
                a_byte = (byte)(a_byte | (1 << a_bitIndex));
            else
                a_byte = (byte)(a_byte & ~(1 << a_bitIndex));
        }

        public static bool IsSet(ushort a_ushort, int a_bitIndex)
        {
            Debug.Assert(a_bitIndex >= 0);
            Debug.Assert(a_bitIndex <= 15);

            return (a_ushort & (1 << a_bitIndex)) != 0;
        }

        public static void SetBit(ref ushort a_ushort, int a_bitIndex, bool a_bitValue)
        {
            Debug.Assert(a_bitIndex >= 0);
            Debug.Assert(a_bitIndex <= 15);

            if (a_bitValue)
                a_ushort = (ushort)(a_ushort | (1 << a_bitIndex));
            else
                a_ushort = (ushort)(a_ushort & ~(1 << a_bitIndex));
        }

        public static bool IsSet(uint a_uint, int a_bitIndex)
        {
            Debug.Assert(a_bitIndex >= 0);
            Debug.Assert(a_bitIndex <= 31);

            return (a_uint & (1 << a_bitIndex)) != 0;
        }

        public static void SetBit(ref uint a_uint, int a_bitIndex, bool a_bitValue)
        {
            Debug.Assert(a_bitIndex >= 0);
            Debug.Assert(a_bitIndex <= 31);

            if (a_bitValue)
                a_uint = a_uint | (1U << a_bitIndex);
            else
                a_uint = a_uint & ~(1U << a_bitIndex);
        }

        public static int Highest(int a_value)
        {
            int h = 0;

            while (a_value != 0)
            {
                a_value >>= 1;
                h++;
            }

            return h;
        }

        public static int Highest(uint a_value)
        {
            int h = 0;

            while (a_value != 0)
            {
                a_value >>= 1;
                h++;
            }

            return h;
        }

        public static int LeastSignificantZero(uint a_value)
        {
            int c = 0;

            for (; ; )
            {
                if ((a_value % 2) == 1)
                    a_value /= 2;
                else
                    break;

                c++;
            }

            return c;
        }

        public static int LeastSignificantZero(int a_value)
        {
            int c = 0;

            for (; ; )
            {
                if ((a_value % 2) == 1)
                    a_value /= 2;
                else
                    break;

                c++;
            }

            return c;
        }

        public static bool IsBase2(int a_value)
        {
            if (a_value == 0)
                return false;

            for (; ; )
            {
                if ((a_value & 1) == 1)
                    break;

                a_value >>= 1;
            }

            return a_value == 1;
        }

        public static bool IsBase2(uint a_value)
        {
            if (a_value == 0)
                return false;

            for (; ; )
            {
                if ((a_value & 1) == 1)
                    break;

                a_value >>= 1;
            }

            return a_value == 1;
        }
    }
}