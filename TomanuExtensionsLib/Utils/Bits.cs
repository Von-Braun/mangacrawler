using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TomanuExtensions
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
    }
}
