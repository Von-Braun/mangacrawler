
using System;
using System.Diagnostics;

namespace HashLib.Crypto.SHA3
{
    internal class Blake224 : Blake256Base
    {
        public Blake224()
            : base(HashLib.HashSize.HashSize224)
        {
        }
    }

    internal class Blake256 : Blake256Base
    {
        public Blake256()
            : base(HashLib.HashSize.HashSize256)
        {
        }
    }

    internal class Blake384 : Blake512Base
    {
        public Blake384()
            : base(HashLib.HashSize.HashSize384)
        {
        }
    }

    internal class Blake512 : Blake512Base
    {
        public Blake512()
            : base(HashLib.HashSize.HashSize512)
        {
        }
    }

    internal abstract class BlakeBase : HashCryptoNotBuildIn
    {
        #region Consts
        protected static readonly byte[] s_padding =
        {
            0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        protected static readonly byte[] s_ending = { 0x00, 0x01, 0x80, 0x81 };

        protected static readonly byte[,] s_sigma = 
        {
            {  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15 } ,
            { 14, 10,  4,  8,  9, 15, 13,  6,  1, 12,  0,  2, 11,  7,  5,  3 } ,
            { 11,  8, 12,  0,  5,  2, 15, 13, 10, 14,  3,  6,  7,  1,  9,  4 } ,
            {  7,  9,  3,  1, 13, 12, 11, 14,  2,  6,  5, 10,  4,  0, 15,  8 } ,
            {  9,  0,  5,  7,  2,  4, 10, 15, 14,  1, 11, 12,  6,  8,  3, 13 } ,
            {  2, 12,  6, 10,  0, 11,  8,  3,  4, 13,  7,  5, 15, 14,  1,  9 } ,
            { 12,  5,  1, 15, 14, 13,  4, 10,  0,  7,  6,  3,  9,  2,  8, 11 } ,
            { 13, 11,  7, 14, 12,  1,  3,  9,  5,  0, 15,  4,  8,  6,  2, 10 } ,
            {  6, 15, 14,  9, 11,  3,  0,  8, 12,  2, 13,  7,  1,  4, 10,  5 } ,
            { 10,  2,  8,  4,  7,  6,  1,  5, 15, 11,  9, 14,  3, 12, 13 , 0 }, 
            {  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15 } ,
            { 14, 10,  4,  8,  9, 15, 13,  6,  1, 12,  0,  2, 11,  7,  5,  3 } ,
            { 11,  8, 12,  0,  5,  2, 15, 13, 10, 14,  3,  6,  7,  1,  9,  4 } ,
            {  7,  9,  3,  1, 13, 12, 11, 14,  2,  6,  5, 10,  4,  0, 15,  8 } ,
            {  9,  0,  5,  7,  2,  4, 10, 15, 14,  1, 11, 12,  6,  8,  3, 13 } ,
            {  2, 12,  6, 10,  0, 11,  8,  3,  4, 13,  7,  5, 15, 14,  1,  9 } ,
            { 12,  5,  1, 15, 14, 13,  4, 10,  0,  7,  6,  3,  9,  2,  8, 11 } ,
            { 13, 11,  7, 14, 12,  1,  3,  9,  5,  0, 15,  4,  8,  6,  2, 10 } ,
            {  6, 15, 14,  9, 11,  3,  0,  8, 12,  2, 13,  7,  1,  4, 10,  5 } ,
            { 10,  2,  8,  4,  7,  6,  1,  5, 15, 11,  9, 14,  3, 12, 13 , 0 }  
        };
        #endregion

        protected bool m_nullt;

        public BlakeBase(HashSize a_hashSize, int a_blockSize)
            : base((int)a_hashSize, a_blockSize)
        {
            Initialize();
        }

        public override void Initialize()
        {
            m_nullt = false;

            base.Initialize();
        }
    };

    internal abstract class Blake256Base : BlakeBase
    {
        #region Consts
        private static readonly uint[] m_c32 = 
        {
            0x243F6A88, 0x85A308D3, 0x13198A2E, 0x03707344, 0xA4093822, 0x299F31D0, 0x082EFA98, 0xEC4E6C89, 
            0x452821E6, 0x38D01377, 0xBE5466CF, 0x34E90C6C, 0xC0AC29B7, 0xC97C50DD, 0x3F84D5B5, 0xB5470917 
        };

        private static readonly uint[] m_initial_state_256 =
        {
            0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19
        };

        private static readonly uint[] m_initial_state_224 =
        {
            0xC1059ED8, 0x367CD507, 0x3070DD17, 0xF70E5939, 0xFFC00B31, 0x68581511, 0x64F98FA7, 0xBEFA4FA4
        };
        #endregion

        private readonly uint[] m_state = new uint[8];

        public Blake256Base(HashSize a_hashSize)
            : base(a_hashSize, 64)
        {
        }

        protected override void TransformBlock(byte[] a_data, int a_index)
        {
            uint[] m = Converters.ConvertBytesToUIntsSwapOrder(a_data, a_index, 64);

            uint v0 = m_state[0];
            uint v1 = m_state[1];
            uint v2 = m_state[2];
            uint v3 = m_state[3];
            uint v4 = m_state[4];
            uint v5 = m_state[5];
            uint v6 = m_state[6];
            uint v7 = m_state[7];

            uint v8 = 0x243F6A88;
            uint v9 = 0x85A308D3;
            uint v10 = 0x13198A2E;
            uint v11 = 0x03707344;
            uint v12 = 0xA4093822;
            uint v13 = 0x299F31D0;
            uint v14 = 0x082EFA98;
            uint v15 = 0xEC4E6C89;

            if (!m_nullt) 
            {
                v12 ^= (uint)(m_processedBytes * 8);
                v13 ^= (uint)(m_processedBytes * 8);
                v14 ^= (uint)((m_processedBytes * 8) >> 32);
                v15 ^= (uint)((m_processedBytes * 8) >> 32);
            }

            for (int round=0; round<10; round++) 
            {
                v0 = (m[s_sigma[round,0]] ^ m_c32[s_sigma[round,1]]) + v0 + v4;
                v12 = v12 ^ v0;
                v12 = (v12 << (32 - 16)) | (v12 >> 16);
                v8 = v8 + v12;
                v4 = v4 ^ v8;
                v4 = (v4 << (32 - 12)) | (v4 >> 12);
                v0 = (m[s_sigma[round,1]] ^ m_c32[s_sigma[round,0]]) + v0 + v4;
                v12 = v12 ^ v0;
                v12 = (v12 << (32 - 8)) | (v12 >> 8);
                v8 = v8 + v12;
                v4 = v4 ^ v8;
                v4 = (v4 << (32 - 7)) | (v4 >> 7);

                v1 = (m[s_sigma[round,2]] ^ m_c32[s_sigma[round,3]]) + v1 + v5;
                v13 = v13 ^ v1;
                v13 = (v13 << (32 - 16)) | (v13 >> 16);
                v9 = v9 + v13;
                v5 = v5 ^ v9;
                v5 = (v5 << (32 - 12)) | (v5 >> 12);
                v1 = (m[s_sigma[round, 3]] ^ m_c32[s_sigma[round, 2]]) + v1 + v5;
                v13 = v13 ^ v1;
                v13 = (v13 << (32 - 8)) | (v13 >> 8);
                v9 = v9 + v13;
                v5 = v5 ^ v9;
                v5 = (v5 << (32 - 7)) | (v5 >> 7);

                v2 = (m[s_sigma[round, 4]] ^ m_c32[s_sigma[round, 5]]) + v2 + v6;
                v14 = v14 ^ v2;
                v14 = (v14 << (32 - 16)) | (v14 >> 16);
                v10 = v10 + v14;
                v6 = v6 ^ v10;
                v6 = (v6 << (32 - 12)) | (v6 >> 12);
                v2 = (m[s_sigma[round, 5]] ^ m_c32[s_sigma[round, 4]]) + v2 + v6;
                v14 = v14 ^ v2;
                v14 = (v14 << (32 - 8)) | (v14 >> 8);
                v10 = v10 + v14;
                v6 = v6 ^ v10;
                v6 = (v6 << (32 - 7)) | (v6 >> 7);

                v3 = (m[s_sigma[round, 6]] ^ m_c32[s_sigma[round, 7]]) + v3 + v7;
                v15 = v15 ^ v3;
                v15 = (v15 << (32 - 16)) | (v15 >> 16);
                v11 = v11 + v15;
                v7 = v7 ^ v11;
                v7 = (v7 << (32 - 12)) | (v7 >> 12);
                v3 = (m[s_sigma[round, 7]] ^ m_c32[s_sigma[round, 6]]) + v3 + v7;
                v15 = v15 ^ v3;
                v15 = (v15 << (32 - 8)) | (v15 >> 8);
                v11 = v11 + v15;
                v7 = v7 ^ v11;
                v7 = (v7 << (32 - 7)) | (v7 >> 7);

                v3 = (m[s_sigma[round, 14]] ^ m_c32[s_sigma[round, 15]]) + v3 + v4;
                v14 = v14 ^ v3;
                v14 = (v14 << (32 - 16)) | (v14 >> 16);
                v9 = v9 + v14;
                v4 = v4 ^ v9;
                v4 = (v4 << (32 - 12)) | (v4 >> 12);
                v3 = (m[s_sigma[round, 15]] ^ m_c32[s_sigma[round, 14]]) + v3 + v4;
                v14 = v14 ^ v3;
                v14 = (v14 << (32 - 8)) | (v14 >> 8);
                v9 = v9 + v14;
                v4 = v4 ^ v9;
                v4 = (v4 << (32 - 7)) | (v4 >> 7);

                v2 = (m[s_sigma[round, 12]] ^ m_c32[s_sigma[round, 13]]) + v2 + v7;
                v13 = v13 ^ v2;
                v13 = (v13 << (32 - 16)) | (v13 >> 16);
                v8 = v8 + v13;
                v7 = v7 ^ v8;
                v7 = (v7 << (32 - 12)) | (v7 >> 12);
                v2 = (m[s_sigma[round, 13]] ^ m_c32[s_sigma[round, 12]]) + v2 + v7;
                v13 = v13 ^ v2;
                v13 = (v13 << (32 - 8)) | (v13 >> 8);
                v8 = v8 + v13;
                v7 = v7 ^ v8;
                v7 = (v7 << (32 - 7)) | (v7 >> 7);

                v0 = (m[s_sigma[round, 8]] ^ m_c32[s_sigma[round, 9]]) + v0 + v5;
                v15 = v15 ^ v0;
                v15 = (v15 << (32 - 16)) | (v15 >> 16);
                v10 = v10 + v15;
                v5 = v5 ^ v10;
                v5 = (v5 << (32 - 12)) | (v5 >> 12);
                v0 = (m[s_sigma[round, 9]] ^ m_c32[s_sigma[round, 8]]) + v0 + v5;
                v15 = v15 ^ v0;
                v15 = (v15 << (32 - 8)) | (v15 >> 8);
                v10 = v10 + v15;
                v5 = v5 ^ v10;
                v5 = (v5 << (32 - 7)) | (v5 >> 7);

                v1 = (m[s_sigma[round, 10]] ^ m_c32[s_sigma[round, 11]]) + v1 + v6;
                v12 = v12 ^ v1;
                v12 = (v12 << (32 - 16)) | (v12 >> 16);
                v11 = v11 + v12;
                v6 = v6 ^ v11;
                v6 = (v6 << (32 - 12)) | (v6 >> 12);
                v1 = (m[s_sigma[round, 11]] ^ m_c32[s_sigma[round, 10]]) + v1 + v6;
                v12 = v12 ^ v1;
                v12 = (v12 << (32 - 8)) | (v12 >> 8);
                v11 = v11 + v12;
                v6 = v6 ^ v11;
                v6 = (v6 << (32 - 7)) | (v6 >> 7);
            }

            m_state[0] ^= v0;
            m_state[1] ^= v1;
            m_state[2] ^= v2;
            m_state[3] ^= v3;
            m_state[4] ^= v4;
            m_state[5] ^= v5;
            m_state[6] ^= v6;
            m_state[7] ^= v7;

            m_state[0] ^= v8;
            m_state[1] ^= v9;
            m_state[2] ^= v10;
            m_state[3] ^= v11;
            m_state[4] ^= v12;
            m_state[5] ^= v13;
            m_state[6] ^= v14;
            m_state[7] ^= v15;
        }

        protected override byte[] GetResult()
        {
            return Converters.ConvertUIntsToBytesSwapOrder(m_state, 0, HashSize/4);
        }

        protected  override void Finish()
        {
            ulong bits = m_processedBytes * 8;

            if (m_buffer.Pos == 55) 
            {
                m_processedBytes -= 1;
                if (HashSize == 28) 
                    TransformBytes(s_ending, 2, 1);
                else
                    TransformBytes(s_ending, 3, 1);
            }
            else 
            {
                if (m_buffer.Pos < 55) 
                {
                    if (m_buffer.Pos == 0) 
                        m_nullt=true;
                    m_processedBytes -= (ulong)(55 - m_buffer.Pos);
                    TransformBytes(s_padding, 0, 55 - m_buffer.Pos);
                }
                else 
                {
                    m_processedBytes -= (ulong)(64 - m_buffer.Pos);
                    TransformBytes(s_padding, 0, 64 - m_buffer.Pos);
                    m_processedBytes -= 55;
                    TransformBytes(s_padding, 1, 55);
                    m_nullt = true;
                }
                if (HashSize == 28) 
                    TransformBytes(s_ending, 0, 1);
                else
                    TransformBytes(s_ending, 1, 1);

                m_processedBytes -= 1;
            }

            m_processedBytes -= 8;

            byte[] msg = new byte[8];
            msg[0] = (byte)(bits >> 56);
            msg[1] = (byte)(bits >> 48);
            msg[2] = (byte)(bits >> 40);
            msg[3] = (byte)(bits >> 32);
            msg[4] = (byte)(bits >> 24);
            msg[5] = (byte)(bits >> 16);
            msg[6] = (byte)(bits >> 8);
            msg[7] = (byte)bits;

            TransformBytes(msg, 0, 8);   
        }

        public override void Initialize()
        {
            if (HashSize == 28) 
                Array.Copy(m_initial_state_224, m_state, 8);
            else
                Array.Copy(m_initial_state_256, m_state, 8);

            base.Initialize();
        }
    }

    internal abstract class Blake512Base : BlakeBase
    {
        #region Consts
        private static readonly ulong[] s_c64 = 
        {
            0x243F6A8885A308D3,0x13198A2E03707344, 0xA4093822299F31D0,0x082EFA98EC4E6C89,
            0x452821E638D01377,0xBE5466CF34E90C6C, 0xC0AC29B7C97C50DD,0x3F84D5B5B5470917,
            0x9216D5D98979FB1B,0xD1310BA698DFB5AC, 0x2FFD72DBD01ADFB7,0xB8E1AFED6A267E96,
            0xBA7C9045F12C7F99,0x24A19947B3916CF7, 0x0801F2E2858EFC16,0x636920D871574E69
        };

        private static readonly ulong[] m_initial_state_384 =
        {
            0xCBBB9D5DC1059ED8, 0x629A292A367CD507, 0x9159015A3070DD17, 0x152FECD8F70E5939,
            0x67332667FFC00B31, 0x8EB44A8768581511, 0xDB0C2E0D64F98FA7, 0x47B5481DBEFA4FA4
        };

        private static readonly ulong[] m_initial_state_512 =
        {
            0x6A09E667F3BCC908, 0xBB67AE8584CAA73B, 0x3C6EF372FE94F82B, 0xA54FF53A5F1D36F1,
            0x510E527FADE682D1, 0x9B05688C2B3E6C1F, 0x1F83D9ABFB41BD6B, 0x5BE0CD19137E2179
        };
        #endregion

        private readonly ulong[] m_state = new ulong[8];

        public Blake512Base(HashSize a_hashSize)
            : base(a_hashSize, 128)
        {
        }

        protected override void TransformBlock(byte[] a_data, int a_index)
        {
            ulong[] m = Converters.ConvertBytesToULongsSwapOrder(a_data, a_index, 128);

            ulong v0 = m_state[0];
            ulong v1 = m_state[1];
            ulong v2 = m_state[2];
            ulong v3 = m_state[3];
            ulong v4 = m_state[4];
            ulong v5 = m_state[5];
            ulong v6 = m_state[6];
            ulong v7 = m_state[7];
            ulong v8 = 0x243F6A8885A308D3;
            ulong v9 = 0x13198A2E03707344;
            ulong v10 = 0xA4093822299F31D0;
            ulong v11 = 0x082EFA98EC4E6C89;
            ulong v12 = 0x452821E638D01377;
            ulong v13 = 0xBE5466CF34E90C6C;
            ulong v14 = 0xC0AC29B7C97C50DD;
            ulong v15 = 0x3F84D5B5B5470917;

            if (!m_nullt) 
            { 
                v12 ^= (m_processedBytes * 8);
                v13 ^= (m_processedBytes * 8);
                v14 ^= 0;
                v15 ^= 0;
            }

            for(int round=0; round<14; round++) 
            {
                v0 = v0 + v4 + (m[s_sigma[round, 0]] ^ s_c64[s_sigma[round, 1]]);
                v12 = v12 ^ v0;
                v12 = (v12 << (64 - 32)) | (v12 >> 32);
                v8 = v8 + v12;
                v4 = v4 ^ v8;
                v4 = (v4 << (64 - 25)) | (v4 >> 25);
                v0 = v0 + v4 + (m[s_sigma[round, 1]] ^ s_c64[s_sigma[round, 0]]);
                v12 = v12 ^ v0;
                v12 = (v12 << (64 - 16)) | (v12 >> 16); 
                v8 = v8 + v12;
                v4 = v4 ^ v8;
                v4 = (v4 << (64 - 11)) | (v4 >> 11);

                v1 = v1 + v5 + (m[s_sigma[round, 2]] ^ s_c64[s_sigma[round, 3]]);
                v13 = v13 ^ v1;
                v13 = (v13 << (64 - 32)) | (v13 >> 32);
                v9 = v9 + v13;
                v5 = v5 ^ v9;
                v5 = (v5 << (64 - 25)) | (v5 >> 25);
                v1 = v1 + v5 + (m[s_sigma[round, 3]] ^ s_c64[s_sigma[round, 2]]);
                v13 = v13 ^ v1;
                v13 = (v13 << (64 - 16)) | (v13 >> 16); 
                v9 = v9 + v13;
                v5 = v5 ^ v9;
                v5 = (v5 << (64 - 11)) | (v5 >> 11);

                v2 = v2 + v6 + (m[s_sigma[round, 4]] ^ s_c64[s_sigma[round, 5]]);
                v14 = v14 ^ v2;
                v14 = (v14 << (64 - 32)) | (v14 >> 32);
                v10 = v10 + v14;
                v6 = v6 ^ v10;
                v6 = (v6 << (64 - 25)) | (v6 >> 25);
                v2 = v2 + v6 + (m[s_sigma[round, 5]] ^ s_c64[s_sigma[round, 4]]);
                v14 = v14 ^ v2;
                v14 = (v14 << (64 - 16)) | (v14 >> 16); 
                v10 = v10 + v14;
                v6 = v6 ^ v10;
                v6 = (v6 << (64 - 11)) | (v6 >> 11);

                v3 = v3 + v7 + (m[s_sigma[round, 6]] ^ s_c64[s_sigma[round, 7]]);
                v15 = v15 ^ v3;
                v15 = (v15 << (64 - 32)) | (v15 >> 32);
                v11 = v11 + v15;
                v7 = v7 ^ v11;
                v7 = (v7 << (64 - 25)) | (v7 >> 25);
                v3 = v3 + v7 + (m[s_sigma[round, 7]] ^ s_c64[s_sigma[round, 6]]);
                v15 = v15 ^ v3;
                v15 = (v15 << (64 - 16)) | (v15 >> 16); 
                v11 = v11 + v15;
                v7 = v7 ^ v11;
                v7 = (v7 << (64 - 11)) | (v7 >> 11);

                v3 = v3 + v4 + (m[s_sigma[round, 14]] ^ s_c64[s_sigma[round, 15]]);
                v14 = v14 ^ v3;
                v14 = (v14 << (64 - 32)) | (v14 >> 32);
                v9 = v9 + v14;
                v4 = v4 ^ v9;
                v4 = (v4 << (64 - 25)) | (v4 >> 25);
                v3 = v3 + v4 + (m[s_sigma[round, 15]] ^ s_c64[s_sigma[round, 14]]);
                v14 = v14 ^ v3;
                v14 = (v14 << (64 - 16)) | (v14 >> 16); 
                v9 = v9 + v14;
                v4 = v4 ^ v9;
                v4 = (v4 << (64 - 11)) | (v4 >> 11);

                v2 = v2 + v7 + (m[s_sigma[round, 12]] ^ s_c64[s_sigma[round, 13]]);
                v13 = v13 ^ v2;
                v13 = (v13 << (64 - 32)) | (v13 >> 32);
                v8 = v8 + v13;
                v7 = v7 ^ v8;
                v7 = (v7 << (64 - 25)) | (v7 >> 25);
                v2 = v2 + v7 + (m[s_sigma[round, 13]] ^ s_c64[s_sigma[round, 12]]);
                v13 = v13 ^ v2;
                v13 = (v13 << (64 - 16)) | (v13 >> 16); 
                v8 = v8 + v13;
                v7 = v7 ^ v8;
                v7 = (v7 << (64 - 11)) | (v7 >> 11);

                v0 = v0 + v5 + (m[s_sigma[round, 8]] ^ s_c64[s_sigma[round, 9]]);
                v15 = v15 ^ v0;
                v15 = (v15 << (64 - 32)) | (v15 >> 32);
                v10 = v10 + v15;
                v5 = v5 ^ v10;
                v5 = (v5 << (64 - 25)) | (v5 >> 25);
                v0 = v0 + v5 + (m[s_sigma[round, 9]] ^ s_c64[s_sigma[round, 8]]);
                v15 = v15 ^ v0;
                v15 = (v15 << (64 - 16)) | (v15 >> 16); 
                v10 = v10 + v15;
                v5 = v5 ^ v10;
                v5 = (v5 << (64 - 11)) | (v5 >> 11);

                v1 = v1 + v6 + (m[s_sigma[round, 10]] ^ s_c64[s_sigma[round, 11]]);
                v12 = v12 ^ v1;
                v12 = (v12 << (64 - 32)) | (v12 >> 32);
                v11 = v11 + v12;
                v6 = v6 ^ v11;
                v6 = (v6 << (64 - 25)) | (v6 >> 25);
                v1 = v1 + v6 + (m[s_sigma[round, 11]] ^ s_c64[s_sigma[round, 10]]);
                v12 = v12 ^ v1;
                v12 = (v12 << (64 - 16)) | (v12 >> 16); 
                v11 = v11 + v12;
                v6 = v6 ^ v11;
                v6 = (v6 << (64 - 11)) | (v6 >> 11);
            }

            m_state[0] ^= v0;
            m_state[1] ^= v1;
            m_state[2] ^= v2;
            m_state[3] ^= v3;
            m_state[4] ^= v4;
            m_state[5] ^= v5;
            m_state[6] ^= v6;
            m_state[7] ^= v7;

            m_state[0] ^= v8;
            m_state[1] ^= v9;
            m_state[2] ^= v10;
            m_state[3] ^= v11;
            m_state[4] ^= v12;
            m_state[5] ^= v13;
            m_state[6] ^= v14;
            m_state[7] ^= v15;
        }

        protected override byte[] GetResult()
        {
            return Converters.ConvertULongsToBytesSwapOrder(m_state, 0, HashSize / 8);
        }

        protected override void Finish()
        {
            ulong bits = m_processedBytes * 8;

            if (m_buffer.Pos == 111) 
            {
                m_processedBytes -= 1;
                if (HashSize == 48) 
                    TransformBytes(s_ending, 2, 1);
                else
                    TransformBytes(s_ending, 3, 1);
            }
            else 
            {
                if (m_buffer.Pos < 111) 
                {
                    if (m_buffer.Pos == 0) 
                        m_nullt=true;
                    m_processedBytes -= (ulong)(111 - m_buffer.Pos);
                    TransformBytes(s_padding, 0, 111 - m_buffer.Pos);
                }
                else 
                {
                    m_processedBytes -= (ulong)(128 - m_buffer.Pos);
                    TransformBytes(s_padding, 0, 128 - m_buffer.Pos);
                    m_processedBytes -= 111;
                    TransformBytes(s_padding, 1, 111);
                    m_nullt = true;
                }
                if (HashSize == 48) 
                    TransformBytes(s_ending, 0, 1);
                else
                    TransformBytes(s_ending, 1, 1);
                m_processedBytes -= 1;
            }

            m_processedBytes -= 16;

            byte[] msg = new byte[16];
            msg[8] = (byte)(bits >> 56);
            msg[9] = (byte)(bits >> 48);
            msg[10] = (byte)(bits >> 40);
            msg[11] = (byte)(bits >> 32);
            msg[12] = (byte)(bits >> 24);
            msg[13] = (byte)(bits >> 16);
            msg[14] = (byte)(bits >> 8);
            msg[15] = (byte)bits;

            TransformBytes(msg, 0, 16);  
        }

        public override void Initialize()
        {
            if (HashSize == 48)
                Array.Copy(m_initial_state_384, m_state, 8);      
            else
                Array.Copy(m_initial_state_512, m_state, 8);

            base.Initialize();
        }
    }
}