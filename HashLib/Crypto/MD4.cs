using System;
using System.Diagnostics;

namespace HashLib.Crypto
{
    internal class MD4 : MDBase
    {
        public MD4() 
            : base(4, 16)
        {
        }

        public override void Initialize()
        {
            m_state[0] = 0x67452301;
            m_state[1] = 0xefcdab89;
            m_state[2] = 0x98badcfe;
            m_state[3] = 0x10325476;

            base.Initialize();
        }

        protected override void TransformBlock(byte[] a_data, int a_index)
        {
            uint[] data = new uint[16];
            Converters.ConvertBytesToUInts(a_data, a_index, BlockSize, data);

            uint a = m_state[0];
            uint b = m_state[1];
            uint c = m_state[2];
            uint d = m_state[3];

            a += data[0] + ((b & c) | ((~b) & d));
            a = a << 3 | a >> (32 - 3);
            d += data[1] + ((a & b) | ((~a) & c));
            d = d << 7 | d >> (32 - 7);
            c += data[2] + ((d & a) | ((~d) & b));
            c = c << 11 | c >> (32 - 11);
            b += data[3] + ((c & d) | ((~c) & a));
            b = b << 19 | b >> (32 - 19);
            a += data[4] + ((b & c) | ((~b) & d));
            a = a << 3 | a >> (32 - 3);
            d += data[5] + ((a & b) | ((~a) & c));
            d = d << 7 | d >> (32 - 7);
            c += data[6] + ((d & a) | ((~d) & b));
            c = c << 11 | c >> (32 - 11);
            b += data[7] + ((c & d) | ((~c) & a));
            b = b << 19 | b >> (32 - 19);
            a += data[8] + ((b & c) | ((~b) & d));
            a = a << 3 | a >> (32 - 3);
            d += data[9] + ((a & b) | ((~a) & c));
            d = d << 7 | d >> (32 - 7);
            c += data[10] + ((d & a) | ((~d) & b));
            c = c << 11 | c >> (32 - 11);
            b += data[11] + ((c & d) | ((~c) & a));
            b = b << 19 | b >> (32 - 19);
            a += data[12] + ((b & c) | ((~b) & d));
            a = a << 3 | a >> (32 - 3);
            d += data[13] + ((a & b) | ((~a) & c));
            d = d << 7 | d >> (32 - 7);
            c += data[14] + ((d & a) | ((~d) & b));
            c = c << 11 | c >> (32 - 11);
            b += data[15] + ((c & d) | ((~c) & a));
            b = b << 19 | b >> (32 - 19);

            a += data[0] + C2 + ((b & (c | d)) | (c & d));
            a = a << 3 | a >> (32 - 3);
            d += data[4] + C2 + ((a & (b | c)) | (b & c));
            d = d << 5 | d >> (32 - 5);
            c += data[8] + C2 + ((d & (a | b)) | (a & b));
            c = c << 9 | c >> (32 - 9);
            b += data[12] + C2 + ((c & (d | a)) | (d & a));
            b = b << 13 | b >> (32 - 13);
            a += data[1] + C2 + ((b & (c | d)) | (c & d));
            a = a << 3 | a >> (32 - 3);
            d += data[5] + C2 + ((a & (b | c)) | (b & c));
            d = d << 5 | d >> (32 - 5);
            c += data[9] + C2 + ((d & (a | b)) | (a & b));
            c = c << 9 | c >> (32 - 9);
            b += data[13] + C2 + ((c & (d | a)) | (d & a));
            b = b << 13 | b >> (32 - 13);
            a += data[2] + C2 + ((b & (c | d)) | (c & d));
            a = a << 3 | a >> (32 - 3);
            d += data[6] + C2 + ((a & (b | c)) | (b & c));
            d = d << 5 | d >> (32 - 5);
            c += data[10] + C2 + ((d & (a | b)) | (a & b));
            c = c << 9 | c >> (32 - 9);
            b += data[14] + C2 + ((c & (d | a)) | (d & a));
            b = b << 13 | b >> (32 - 13);
            a += data[3] + C2 + ((b & (c | d)) | (c & d));
            a = a << 3 | a >> (32 - 3);
            d += data[7] + C2 + ((a & (b | c)) | (b & c));
            d = d << 5 | d >> (32 - 5);
            c += data[11] + C2 + ((d & (a | b)) | (a & b));
            c = c << 9 | c >> (32 - 9);
            b += data[15] + C2 + ((c & (d | a)) | (d & a));
            b = b << 13 | b >> (32 - 13);

            a += data[0] + C4 + (b ^ c ^ d);
            a = a << 3 | a >> (32 - 3);
            d += data[8] + C4 + (a ^ b ^ c);
            d = d << 9 | d >> (32 - 9);
            c += data[4] + C4 + (d ^ a ^ b);
            c = c << 11 | c >> (32 - 11);
            b += data[12] + C4 + (c ^ d ^ a);
            b = b << 15 | b >> (32 - 15);
            a += data[2] + C4 + (b ^ c ^ d);
            a = a << 3 | a >> (32 - 3);
            d += data[10] + C4 + (a ^ b ^ c);
            d = d << 9 | d >> (32 - 9);
            c += data[6] + C4 + (d ^ a ^ b);
            c = c << 11 | c >> (32 - 11);
            b += data[14] + C4 + (c ^ d ^ a);
            b = b << 15 | b >> (32 - 15);
            a += data[1] + C4 + (b ^ c ^ d);
            a = a << 3 | a >> (32 - 3);
            d += data[9] + C4 + (a ^ b ^ c);
            d = d << 9 | d >> (32 - 9);
            c += data[5] + C4 + (d ^ a ^ b);
            c = c << 11 | c >> (32 - 11);
            b += data[13] + C4 + (c ^ d ^ a);
            b = b << 15 | b >> (32 - 15);
            a += data[3] + C4 + (b ^ c ^ d);
            a = a << 3 | a >> (32 - 3);
            d += data[11] + C4 + (a ^ b ^ c);
            d = d << 9 | d >> (32 - 9);
            c += data[7] + C4 + (d ^ a ^ b);
            c = c << 11 | c >> (32 - 11);
            b += data[15] + C4 + (c ^ d ^ a);
            b = b << 15 | b >> (32 - 15);

            m_state[0] += a;
            m_state[1] += b;
            m_state[2] += c;
            m_state[3] += d;
        }
    }
}
