using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

/* C# Version Copyright (C) 2001-2004 Akihilo Kramot (Takel).  */
/* C# porting from a C-program for MT19937, originaly coded by */
/* Takuji Nishimura, considering the suggestions by            */
/* Topher Cooper and Marc Rieffel in July-Aug. 1997.           */
/* This library is free software under the Artistic license:   */
/*                                                             */
/* You can find the original C-program at                      */
/*     http://www.math.keio.ac.jp/~matumoto/mt.html            */
/*                                                             */

namespace TomanuExtensions.Utils
{
    public class MersenneTwister : System.Random
    {
        /* Period parameters */
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908b0df; /* constant vector a */
        private const uint UPPER_MASK = 0x80000000; /* most significant w-r bits */
        private const uint LOWER_MASK = 0x7fffffff; /* least significant r bits */

        /* Tempering parameters */
        private const uint TEMPERING_MASK_B = 0x9d2c5680;
        private const uint TEMPERING_MASK_C = 0xefc60000;

        private static uint TEMPERING_SHIFT_U(uint y) { return (y >> 11); }

        private static uint TEMPERING_SHIFT_S(uint y) { return (y << 7); }

        private static uint TEMPERING_SHIFT_T(uint y) { return (y << 15); }

        private static uint TEMPERING_SHIFT_L(uint y) { return (y >> 18); }

        private uint[] mt = new uint[N]; /* the array for the state vector  */

        private short mti;

        private static uint[] mag01 = { 0x0, MATRIX_A };

        /* initializing the array with a NONZERO seed */

        public MersenneTwister(uint seed)
        {
            /* setting initial seeds to mt[N] using         */
            /* the generator Line 25 of Table 1 in          */
            /* [KNUTH 1981, The Art of Computer Programming */
            /*    Vol. 2 (2nd Ed.), pp102]                  */
            mt[0] = seed & 0xffffffffU;
            for (mti = 1; mti < N; ++mti)
            {
                mt[mti] = (69069 * mt[mti - 1]) & 0xffffffffU;
            }
        }

        public MersenneTwister()
            : this((uint)System.Environment.TickCount)
        {
        }

        protected uint GenerateUInt()
        {
            uint y;

            /* mag01[x] = x * MATRIX_A  for x=0,1 */
            if (mti >= N) /* generate N words at one time */
            {
                short kk = 0;

                for (; kk < N - M; ++kk)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1];
                }

                for (; kk < N - 1; ++kk)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1];
                }

                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1];

                mti = 0;
            }

            y = mt[mti++];
            y ^= TEMPERING_SHIFT_U(y);
            y ^= TEMPERING_SHIFT_S(y) & TEMPERING_MASK_B;
            y ^= TEMPERING_SHIFT_T(y) & TEMPERING_MASK_C;
            y ^= TEMPERING_SHIFT_L(y);

            return y;
        }

        public uint NextUInt()
        {
            return GenerateUInt();
        }

        public uint NextUInt(uint a_max_value_exluded)
        {
            return (uint)(GenerateUInt() / ((double)uint.MaxValue / a_max_value_exluded));
        }

        public int NextInt(int a_max_value_exluded)
        {
            Debug.Assert(a_max_value_exluded > 0);
            return (int)NextUInt((uint)a_max_value_exluded);
        }

        public ushort NextUShort(ushort a_max_value_exluded)
        {
            return (ushort)(GenerateUInt() / ((double)ushort.MaxValue / a_max_value_exluded));
        }

        public T NextElement<T>(IEnumerable<T> a_enum)
        {
            return a_enum.ElementAt(NextInt(a_enum.Count()));
        }

        public T NextElement<T>(T[] a_array)
        {
            return a_array[NextInt(a_array.Length)];
        }

        public T NextElement<T>(IList<T> a_list)
        {
            return a_list[NextInt(a_list.Count)];
        }

        public uint NextUInt(uint a_min_value_include, uint a_max_value_exluded)
        {
            Debug.Assert(a_min_value_include < a_max_value_exluded);

            return (uint)(GenerateUInt() / ((double)uint.MaxValue / (a_max_value_exluded - a_min_value_include)) + a_min_value_include);
        }

        public int NextInt(int a_min_value_include, int a_max_value_exluded) 
        {
            Debug.Assert(a_min_value_include < a_max_value_exluded);

            return (int)((long)a_min_value_include + NextUInt((uint)((long)a_max_value_exluded - (long)a_min_value_include)));
        }

        public override int Next()
        {
            return Next(int.MaxValue);
        }

        public override int Next(int a_max_value_exluded)
        {
            Debug.Assert(a_max_value_exluded > 0);

            return (int)(NextDouble() * a_max_value_exluded);
        }

        public override int Next(int a_min_value_include, int a_max_value_exluded)
        {
            Debug.Assert(a_max_value_exluded >= a_min_value_include);

            if (a_max_value_exluded == a_min_value_include)
            {
                return a_min_value_include;
            }
            else
            {
                return Next(a_max_value_exluded - a_min_value_include) + a_min_value_include;
            }
        }

        public override void NextBytes(byte[] buffer) 
        {
            int bufLen = buffer.Length;

            for (int idx = 0; idx < bufLen; ++idx)
                buffer[idx] = (byte)Next(256);
        }

        public override double NextDouble()
        {
            return (double)GenerateUInt() / ((ulong)uint.MaxValue + 1);
        }

        public float NextFloat()
        {
            return (float)GenerateUInt() / ((ulong)uint.MaxValue + 1);
        }

        public byte NextByte()
        {
            return (byte)NextUInt(byte.MaxValue);
        }

        public char NextChar()
        {
            return (char)NextUInt(char.MaxValue);
        }

        public short NextShort()
        {
            return (short)Next(short.MinValue, short.MaxValue);
        }

        public ushort NextUShort()
        {
            return (ushort)NextUInt(ushort.MaxValue);
        }

        public int NextInt()
        {
            return (int)GenerateUInt();
        }

        public long NextLong()
        {
            return ((long)NextUInt() << 32) | NextUInt();
        }

        public ulong NextULong()
        {
            return ((ulong)NextUInt() << 32) | NextUInt();
        }

        public byte[] NextBytes(int a_length)
        {
            byte[] result = new byte[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextByte();
            return result;
        }

        public char[] NextChars(int a_length)
        {
            char[] result = new char[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextChar();
            return result;
        }

        public short[] NextShorts(int a_length)
        {
            short[] result = new short[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextShort();
            return result;
        }

        public ushort[] NextUShorts(int a_length)
        {
            ushort[] result = new ushort[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextUShort();
            return result;
        }

        public int[] NextInts(int a_length)
        {
            int[] result = new int[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = Next();
            return result;
        }

        public uint[] NextUInts(int a_length)
        {
            uint[] result = new uint[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextUInt();
            return result;
        }

        public long[] NextLongs(int a_length)
        {
            long[] result = new long[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextLong();
            return result;
        }

        public ulong[] NextULongs(int a_length)
        {
            ulong[] result = new ulong[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextULong();
            return result;
        }

        public string NextString(int a_length)
        {
            return new string(NextChars(a_length));
        }

        public string NextText(int a_length)
        {
            char[] ar = new char[a_length];

            const int MORE_SPACES = 5;

            for (int i = 0; i < ar.Length; i++)
            {
                for (;;)
                {
                    int c = NextInt(MORE_SPACES + ('9' - '0' + 1) + ('z' - 'a' + 1) + ('Z' - 'A' + 1));

                    if (c < MORE_SPACES)
                    {
                        if (i == 0)
                            continue;
                        if (i == ar.Length - 1)
                            continue;
                        ar[i] = ' ';
                        break;
                    }
                    c = c - MORE_SPACES;
                    if (c < '9' - '0' + 1)
                    {
                        ar[i] = (char)('0' + c);
                        break;
                    }
                    c = c - ('9' - '0' + 1);
                    if (c < 'Z' - 'A' + 1)
                    {
                        ar[i] = (char)('A' + c);
                        break;
                    }
                    c = c - ('Z' - 'A' + 1);
                    if (c < 'z' - 'a' + 1)
                    {
                        ar[i] = (char)('a' + c);
                        break;
                    }
                }

                Debug.Assert((ar[i] == ' ') || 
                             (ar[i] >= '0' && ar[i] <= '9') || 
                             (ar[i] >= 'a' && ar[i] <= 'z') || 
                             (ar[i] >= 'A' && ar[i] <= 'Z'));
            }

            return new string(ar);
        }

        public double NextDoubleFull()
        {
            return BitConverter.Int64BitsToDouble(NextLong());
        }

        public float NextFloatFull()
        {
            return Converters.ConvertBytesToFloat(BitConverter.GetBytes(NextUInt()), 0);
        }

        public double[] NextDoublesFull(int a_length)
        {
            double[] result = new double[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextDoubleFull();
            return result;
        }

        public double[] NextDoublesFullSafe(int a_length)
        {
            double[] result = new double[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextDoubleFullSafe();
            return result;
        }

        public double[] NextDoubles(int a_length)
        {
            double[] result = new double[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextDouble();
            return result;
        }

        public float[] NextFloatsFull(int a_length)
        {
            float[] result = new float[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextFloatFull();
            return result;
        }

        public float[] NextFloatsFullSafe(int a_length)
        {
            float[] result = new float[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextFloatFullSafe();
            return result;
        }

        public float[] NextFloats(int a_length)
        {
            float[] result = new float[a_length];
            for (int i = 0; i < a_length; i++)
                result[i] = NextFloat();
            return result;
        }

        public double NextDoubleFullSafe()
        {
            byte[] ar = new byte[8];

            for (; ; )
            {
                double d = BitConverter.ToDouble(ar, 0);

                if (Double.IsNaN(d))
                    continue;

                float f1 = BitConverter.ToSingle(ar, 0);

                if (Single.IsNaN(f1))
                    continue;

                float f2 = BitConverter.ToSingle(ar, 4);

                if (Single.IsNaN(f1))
                    continue;

                return d;
            }
        }

        public float NextFloatFullSafe()
        {
            for (; ; )
            {
                float f = NextFloatFull();
                if (!Single.IsNaN(f))
                    return f;
            }
        }
    }
}