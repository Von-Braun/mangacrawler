using System;
using System.Diagnostics;

namespace HashLib.Crypto
{
    internal class RIPEMD128 : MDBase
    {
        public RIPEMD128() 
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

            uint aa, bb, cc, dd;
            uint a = aa = m_state[0];
            uint b = bb = m_state[1];
            uint c = cc = m_state[2];
            uint d = dd = m_state[3];

            a += data[0] + (b ^ c ^ d);
            a = (a << 11) | (a >> (32 - 11));
            d += data[1] + (a ^ b ^ c);
            d = (d << 14) | (d >> (32 - 14));
            c += data[2] + (d ^ a ^ b);
            c = (c << 15) | (c >> (32 - 15));
            b += data[3] + (c ^ d ^ a);
            b = (b << 12) | (b >> (32 - 12));
            a += data[4] + (b ^ c ^ d);
            a = (a << 5) | (a >> (32 - 5));
            d += data[5] + (a ^ b ^ c);
            d = (d << 8) | (d >> (32 - 8));
            c += data[6] + (d ^ a ^ b);
            c = (c << 7) | (c >> (32 - 7));
            b += data[7] + (c ^ d ^ a);
            b = (b << 9) | (b >> (32 - 9));
            a += data[8] + (b ^ c ^ d);
            a = (a << 11) | (a >> (32 - 11));
            d += data[9] + (a ^ b ^ c);
            d = (d << 13) | (d >> (32 - 13));
            c += data[10] + (d ^ a ^ b);
            c = (c << 14) | (c >> (32 - 14));
            b += data[11] + (c ^ d ^ a);
            b = (b << 15) | (b >> (32 - 15));
            a += data[12] + (b ^ c ^ d);
            a = (a << 6) | (a >> (32 - 6));
            d += data[13] + (a ^ b ^ c);
            d = (d << 7) | (d >> (32 - 7));
            c += data[14] + (d ^ a ^ b);
            c = (c << 9) | (c >> (32 - 9));
            b += data[15] + (c ^ d ^ a);
            b = (b << 8) | (b >> (32 - 8));

            a += data[7] + C2 + ((b & c) | (~b & d));
            a = (a << 7) | (a >> (32 - 7));
            d += data[4] + C2 + ((a & b) | (~a & c));
            d = (d << 6) | (d >> (32 - 6));
            c += data[13] + C2 + ((d & a) | (~d & b));
            c = (c << 8) | (c >> (32 - 8));
            b += data[1] + C2 + ((c & d) | (~c & a));
            b = (b << 13) | (b >> (32 - 13));
            a += data[10] + C2 + ((b & c) | (~b & d));
            a = (a << 11) | (a >> (32 - 11));
            d += data[6] + C2 + ((a & b) | (~a & c));
            d = (d << 9) | (d >> (32 - 9));
            c += data[15] + C2 + ((d & a) | (~d & b));
            c = (c << 7) | (c >> (32 - 7));
            b += data[3] + C2 + ((c & d) | (~c & a));
            b = (b << 15) | (b >> (32 - 15));
            a += data[12] + C2 + ((b & c) | (~b & d));
            a = (a << 7) | (a >> (32 - 7));
            d += data[0] + C2 + ((a & b) | (~a & c));
            d = (d << 12) | (d >> (32 - 12));
            c += data[9] + C2 + ((d & a) | (~d & b));
            c = (c << 15) | (c >> (32 - 15));
            b += data[5] + C2 + ((c & d) | (~c & a));
            b = (b << 9) | (b >> (32 - 9));
            a += data[2] + C2 + ((b & c) | (~b & d));
            a = (a << 11) | (a >> (32 - 11));
            d += data[14] + C2 + ((a & b) | (~a & c));
            d = (d << 7) | (d >> (32 - 7));
            c += data[11] + C2 + ((d & a) | (~d & b));
            c = (c << 13) | (c >> (32 - 13));
            b += data[8] + C2 + ((c & d) | (~c & a));
            b = (b << 12) | (b >> (32 - 12));

            a += data[3] + C4 + ((b | ~c) ^ d);
            a = (a << 11) | (a >> (32 - 11));
            d += data[10] + C4 + ((a | ~b) ^ c);
            d = (d << 13) | (d >> (32 - 13));
            c += data[14] + C4 + ((d | ~a) ^ b);
            c = (c << 6) | (c >> (32 - 6));
            b += data[4] + C4 + ((c | ~d) ^ a);
            b = (b << 7) | (b >> (32 - 7));
            a += data[9] + C4 + ((b | ~c) ^ d);
            a = (a << 14) | (a >> (32 - 14));
            d += data[15] + C4 + ((a | ~b) ^ c);
            d = (d << 9) | (d >> (32 - 9));
            c += data[8] + C4 + ((d | ~a) ^ b);
            c = (c << 13) | (c >> (32 - 13));
            b += data[1] + C4 + ((c | ~d) ^ a);
            b = (b << 15) | (b >> (32 - 15));
            a += data[2] + C4 + ((b | ~c) ^ d);
            a = (a << 14) | (a >> (32 - 14));
            d += data[7] + C4 + ((a | ~b) ^ c);
            d = (d << 8) | (d >> (32 - 8));
            c += data[0] + C4 + ((d | ~a) ^ b);
            c = (c << 13) | (c >> (32 - 13));
            b += data[6] + C4 + ((c | ~d) ^ a);
            b = (b << 6) | (b >> (32 - 6));
            a += data[13] + C4 + ((b | ~c) ^ d);
            a = (a << 5) | (a >> (32 - 5));
            d += data[11] + C4 + ((a | ~b) ^ c);
            d = (d << 12) | (d >> (32 - 12));
            c += data[5] + C4 + ((d | ~a) ^ b);
            c = (c << 7) | (c >> (32 - 7));
            b += data[12] + C4 + ((c | ~d) ^ a);
            b = (b << 5) | (b >> (32 - 5));

            a += data[1] + C6 + ((b & d) | (c & ~d));
            a = (a << 11) | (a >> (32 - 11));
            d += data[9] + C6 + ((a & c) | (b & ~c));
            d  = (d  << 12) | (d >> (32 - 12));
            c += data[11] + C6 + ((d & b) | (a & ~b));
            c  = (c  << 14) | (c >> (32 - 14));
            b += data[10] + C6 + ((c & a) | (d & ~a));
            b  = (b  << 15) | (b >> (32 - 15));
            a += data[0] + C6 + ((b & d) | (c & ~d));
            a  = (a  << 14) | (a >> (32 - 14));
            d += data[8] + C6 + ((a & c) | (b & ~c));
            d  = (d  << 15) | (d >> (32 - 15));
            c += data[12] + C6 + ((d & b) | (a & ~b));
            c  = (c  << 9) | (c >> (32 - 9));
            b += data[4] + C6 + ((c & a) | (d & ~a));
            b  = (b  << 8) | (b >> (32 - 8));
            a += data[13] + C6 + ((b & d) | (c & ~d));
            a  = (a  << 9) | (a >> (32 - 9));
            d += data[3] + C6 + ((a & c) | (b & ~c));
            d  = (d  << 14) | (d >> (32 - 14));
            c += data[7] + C6 + ((d & b) | (a & ~b));
            c  = (c  << 5) | (c >> (32 - 5));
            b += data[15] + C6 + ((c & a) | (d & ~a));
            b  = (b  << 6) | (b >> (32 - 6));
            a += data[14] + C6 + ((b & d) | (c & ~d));
            a  = (a  << 8) | (a >> (32 - 8));
            d += data[5] + C6 + ((a & c) | (b & ~c));
            d  = (d  << 6) | (d >> (32 - 6));
            c += data[6] + C6 + ((d & b) | (a & ~b));
            c  = (c  << 5) | (c >> (32 - 5));
            b += data[2] + C6 + ((c & a) | (d & ~a));
            b  = (b  << 12) | (b >> (32 - 12));

            aa += data[5] + C1 + ((bb & dd) | (cc & ~dd));
            aa = (aa << 8) | (aa >> (32 - 8));
            dd += data[14] + C1 + ((aa & cc) | (bb & ~cc));
            dd = (dd << 9) | (dd >> (32 - 9));
            cc += data[7] + C1 + ((dd & bb) | (aa & ~bb));
            cc = (cc << 9) | (cc >> (32 - 9));
            bb += data[0] + C1 + ((cc & aa) | (dd & ~aa));
            bb = (bb << 11) | (bb >> (32 - 11));
            aa += data[9] + C1 + ((bb & dd) | (cc & ~dd));
            aa  = (aa  << 13) | (aa >> (32 - 13));
            dd += data[2] + C1 + ((aa & cc) | (bb & ~cc));
            dd  = (dd  << 15) | (dd >> (32 - 15));
            cc += data[11] + C1 + ((dd & bb) | (aa & ~bb));
            cc  = (cc  << 15) | (cc >> (32 - 15));
            bb += data[4] + C1 + ((cc & aa) | (dd & ~aa));
            bb  = (bb  << 5) | (bb >> (32 - 5));
            aa += data[13] + C1 + ((bb & dd) | (cc & ~dd));
            aa  = (aa  << 7) | (aa >> (32 - 7));
            dd += data[6] + C1 + ((aa & cc) | (bb & ~cc));
            dd  = (dd  << 7) | (dd >> (32 - 7));
            cc += data[15] + C1 + ((dd & bb) | (aa & ~bb));
            cc  = (cc  << 8) | (cc >> (32 - 8));
            bb += data[8] + C1 + ((cc & aa) | (dd & ~aa));
            bb  = (bb  << 11) | (bb >> (32 - 11));
            aa += data[1] + C1 + ((bb & dd) | (cc & ~dd));
            aa  = (aa  << 14) | (aa >> (32 - 14));
            dd += data[10] + C1 + ((aa & cc) | (bb & ~cc));
            dd  = (dd  << 14) | (dd >> (32 - 14));
            cc += data[3] + C1 + ((dd & bb) | (aa & ~bb));
            cc  = (cc  << 12) | (cc >> (32 - 12));
            bb += data[12] + C1 + ((cc & aa) | (dd & ~aa));
            bb  = (bb  << 6) | (bb >> (32 - 6));

            aa += data[6] + C3 + ((bb | ~cc) ^ dd);
            aa = (aa << 9) | (aa >> (32 - 9));
            dd += data[11] + C3 + ((aa | ~bb) ^ cc);
            dd = (dd << 13) | (dd >> (32 - 13));
            cc += data[3] + C3 + ((dd | ~aa) ^ bb);
            cc = (cc << 15) | (cc >> (32 - 15));
            bb += data[7] + C3 + ((cc | ~dd) ^ aa);
            bb = (bb << 7) | (bb >> (32 - 7));
            aa += data[0] + C3 + ((bb | ~cc) ^ dd);
            aa = (aa << 12) | (aa >> (32 - 12));
            dd += data[13] + C3 + ((aa | ~bb) ^ cc);
            dd = (dd << 8) | (dd >> (32 - 8));
            cc += data[5] + C3 + ((dd | ~aa) ^ bb);
            cc = (cc << 9) | (cc >> (32 - 9));
            bb += data[10] + C3 + ((cc | ~dd) ^ aa);
            bb = (bb << 11) | (bb >> (32 - 11));
            aa += data[14] + C3 + ((bb | ~cc) ^ dd);
            aa = (aa << 7) | (aa >> (32 - 7));
            dd += data[15] + C3 + ((aa | ~bb) ^ cc);
            dd = (dd << 7) | (dd >> (32 - 7));
            cc += data[8] + C3 + ((dd | ~aa) ^ bb);
            cc = (cc << 12) | (cc >> (32 - 12));
            bb += data[12] + C3 + ((cc | ~dd) ^ aa);
            bb = (bb << 7) | (bb >> (32 - 7));
            aa += data[4] + C3 + ((bb | ~cc) ^ dd);
            aa = (aa << 6) | (aa >> (32 - 6));
            dd += data[9] + C3 + ((aa | ~bb) ^ cc);
            dd = (dd << 15) | (dd >> (32 - 15));
            cc += data[1] + C3 + ((dd | ~aa) ^ bb);
            cc = (cc << 13) | (cc >> (32 - 13));
            bb += data[2] + C3 + ((cc | ~dd) ^ aa);
            bb = (bb << 11) | (bb >> (32 - 11));

            aa += data[15] + C5 + ((bb & cc) | (~bb & dd));
            aa = (aa << 9) | (aa >> (32 - 9));
            dd += data[5] + C5 + ((aa & bb) | (~aa & cc));
            dd = (dd << 7) | (dd >> (32 - 7));
            cc += data[1] + C5 + ((dd & aa) | (~dd & bb));
            cc = (cc << 15) | (cc >> (32 - 15));
            bb += data[3] + C5 + ((cc & dd) | (~cc & aa));
            bb = (bb << 11) | (bb >> (32 - 11));
            aa += data[7] + C5 + ((bb & cc) | (~bb & dd));
            aa = (aa << 8) | (aa >> (32 - 8));
            dd += data[14] + C5 + ((aa & bb) | (~aa & cc));
            dd = (dd << 6) | (dd >> (32 - 6));
            cc += data[6] + C5 + ((dd & aa) | (~dd & bb));
            cc = (cc << 6) | (cc >> (32 - 6));
            bb += data[9] + C5 + ((cc & dd) | (~cc & aa));
            bb = (bb << 14) | (bb >> (32 - 14));
            aa += data[11] + C5 + ((bb & cc) | (~bb & dd));
            aa = (aa << 12) | (aa >> (32 - 12));
            dd += data[8] + C5 + ((aa & bb) | (~aa & cc));
            dd = (dd << 13) | (dd >> (32 - 13));
            cc += data[12] + C5 + ((dd & aa) | (~dd & bb));
            cc = (cc << 5) | (cc >> (32 - 5));
            bb += data[2] + C5 + ((cc & dd) | (~cc & aa));
            bb = (bb << 14) | (bb >> (32 - 14));
            aa += data[10] + C5 + ((bb & cc) | (~bb & dd));
            aa = (aa << 13) | (aa >> (32 - 13));
            dd += data[0] + C5 + ((aa & bb) | (~aa & cc));
            dd = (dd << 13) | (dd >> (32 - 13));
            cc += data[4] + C5 + ((dd & aa) | (~dd & bb));
            cc = (cc << 7) | (cc >> (32 - 7));
            bb += data[13] + C5 + ((cc & dd) | (~cc & aa));
            bb = (bb << 5) | (bb >> (32 - 5));

            aa += data[8] + (bb ^ cc ^ dd);
            aa = (aa << 15) | (aa >> (32 - 15));
            dd += data[6] + (aa ^ bb ^ cc);
            dd = (dd << 5) | (dd >> (32 - 5));
            cc += data[4] + (dd ^ aa ^ bb);
            cc = (cc << 8) | (cc >> (32 - 8));
            bb += data[1] + (cc ^ dd ^ aa);
            bb = (bb << 11) | (bb >> (32 - 11));
            aa += data[3] + (bb ^ cc ^ dd);
            aa = (aa << 14) | (aa >> (32 - 14));
            dd += data[11] + (aa ^ bb ^ cc);
            dd = (dd << 14) | (dd >> (32 - 14));
            cc += data[15] + (dd ^ aa ^ bb);
            cc = (cc << 6) | (cc >> (32 - 6));
            bb += data[0] + (cc ^ dd ^ aa);
            bb = (bb << 14) | (bb >> (32 - 14));
            aa += data[5] + (bb ^ cc ^ dd);
            aa = (aa << 6) | (aa >> (32 - 6));
            dd += data[12] + (aa ^ bb ^ cc);
            dd = (dd << 9) | (dd >> (32 - 9));
            cc += data[2] + (dd ^ aa ^ bb);
            cc = (cc << 12) | (cc >> (32 - 12));
            bb += data[13] + (cc ^ dd ^ aa);
            bb = (bb << 9) | (bb >> (32 - 9));
            aa += data[9] + (bb ^ cc ^ dd);
            aa = (aa << 12) | (aa >> (32 - 12));
            dd += data[7] + (aa ^ bb ^ cc);
            dd = (dd << 5) | (dd >> (32 - 5));
            cc += data[10] + (dd ^ aa ^ bb);
            cc = (cc << 15) | (cc >> (32 - 15));
            bb += data[14] + (cc ^ dd ^ aa);
            bb = (bb << 8) | (bb >> (32 - 8));

            dd += c + m_state[1];
            m_state[1] = m_state[2] + d + aa;
            m_state[2] = m_state[3] + a + bb;
            m_state[3] = m_state[0] + b + cc;
            m_state[0] = dd;
        }
    }
}
