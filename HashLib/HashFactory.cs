using System;
using System.Reflection;
using System.Linq;
using System.Collections.ObjectModel;

namespace HashLib
{
    public static class HashFactory
    {
        public static class Hash32
        {
            public static IHash CreateAP()
            {
                return new HashLib.Hash32.AP();
            }
            
            public static IHash CreateBernstein()
            {
                return new HashLib.Hash32.Bernstein();
            }
            
            public static IHash CreateBernstein1()
            {
                return new HashLib.Hash32.Bernstein1();
            }
            
            public static IHash CreateBKDR()
            {
                return new HashLib.Hash32.BKDR();
            }
            
            public static IHash CreateDEK()
            {
                return new HashLib.Hash32.DEK();
            }
            
            public static IHash CreateDJB()
            {
                return new HashLib.Hash32.DJB();
            }
            
            public static IHash CreateDotNet()
            {
                return new HashLib.Hash32.DotNet();
            }
            
            public static IHash CreateELF()
            {
                return new HashLib.Hash32.ELF();
            }
            
            public static IHash CreateFNV()
            {
                return new HashLib.Hash32.FNV();
            }
            
            public static IHash CreateFNV1a()
            {
                return new HashLib.Hash32.FNV1a();
            }
            
            public static IHash CreateJenkins3()
            {
                return new HashLib.Hash32.Jenkins3();
            }
            
            public static IHash CreateJS()
            {
                return new HashLib.Hash32.JS();
            }
            
            public static IHash CreateMurmur2()
            {
                return new HashLib.Hash32.Murmur2();
            }
            
            public static IHash CreateOneAtTime()
            {
                return new HashLib.Hash32.OneAtTime();
            }
            
            public static IHash CreatePJW()
            {
                return new HashLib.Hash32.PJW();
            }
            
            public static IHash CreateRotating()
            {
                return new HashLib.Hash32.Rotating();
            }
            
            public static IHash CreateRS()
            {
                return new HashLib.Hash32.RS();
            }
            
            public static IHash CreateSDBM()
            {
                return new HashLib.Hash32.SDBM();
            }
            
            public static IHash CreateShiftAndXor()
            {
                return new HashLib.Hash32.ShiftAndXor();
            }

            public static IHash CreateSuperFast()
            {
                return new HashLib.Hash32.SuperFast();
            }
        }

        public static class Checksum
        {
            /// <summary>
            /// IEEE 802.3, polynomial = 0xEDB88320
            /// </summary>
            /// <returns></returns>
            public static IHash CreateCRC32a()
            {
                return new HashLib.Checksum.CRC32a();
            }

            /// <summary>
            /// Castagnoli, polynomial = 0x82F63B78
            /// </summary>
            /// <returns></returns>
            public static IHash CreateCRC32b()
            {
                return new HashLib.Checksum.CRC32b();
            }

            /// <summary>
            /// Koopman, polynomial = 0xEB31D82E
            /// </summary>
            /// <returns></returns>
            public static IHash CreateCRC32c()
            {
                return new HashLib.Checksum.CRC32c();
            }


            /// <summary>
            /// Q, polynomial = 0xD5828281
            /// </summary>
            /// <returns></returns>
            public static IHash CreateCRC32d()
            {
                return new HashLib.Checksum.CRC32d();
            }

            public static IHash CreateCRC32(uint a_polynomial, uint a_initial_value = uint.MaxValue, uint a_final_xor = uint.MaxValue)
            {
                return new HashLib.Checksum.CRC32(a_polynomial, a_initial_value, a_final_xor);
            }

            public static IHash CreateAdler32()
            {
                return new HashLib.Checksum.Adler32();
            }

            /// <summary>
            /// ECMA 182, polynomial = 0xC96C5795D7870F42
            /// </summary>
            /// <returns></returns>
            public static IHash CreateCRC64a()
            {
                return new HashLib.Checksum.CRC64a();
            }

            /// <summary>
            /// ISO, polynomial = 0xD800000000000000
            /// </summary>
            /// <returns></returns>
            public static IHash CreateCRC64b()
            {
                return new HashLib.Checksum.CRC64b();
            }

            public static IHash CreateCRC64(ulong a_polynomial, ulong a_initial_value = ulong.MaxValue, ulong a_final_xor = ulong.MaxValue)
            {
                return new HashLib.Checksum.CRC64(a_polynomial, a_initial_value, a_final_xor);
            }
        }

        public static class Hash64
        {
            public static IHash CreateFNV1a()
            {
                return new HashLib.Hash64.FNV1a64();
            }

            public static IHash CreateFNV()
            {
                return new HashLib.Hash64.FNV64();
            }

            public static IHash CreateMurmur2()
            {
                return new HashLib.Hash64.Murmur2_64();
            }
        }

        public static class Crypto
        {
            public static class SHA3
            {
                public static IHash CreateJH224()
                {
                    return new HashLib.Crypto.SHA3.JH224();
                }

                public static IHash CreateJH256()
                {
                    return new HashLib.Crypto.SHA3.JH256();
                }

                public static IHash CreateJH384()
                {
                    return new HashLib.Crypto.SHA3.JH384();
                }

                public static IHash CreateJH512()
                {
                    return new HashLib.Crypto.SHA3.JH512();
                }

                public static IHash CreateJH(HashLib.HashSize a_hashSize)
                {
                    switch (a_hashSize)
                    {
                        case HashLib.HashSize.HashSize224: return CreateJH224();
                        case HashLib.HashSize.HashSize256: return CreateJH256();
                        case HashLib.HashSize.HashSize384: return CreateJH384();
                        case HashLib.HashSize.HashSize512: return CreateJH512();
                        default: throw new ArgumentException();
                    }
                }

                public static IHash CreateBlake224()
                {
                    return new HashLib.Crypto.SHA3.Blake224();
                }

                public static IHash CreateBlake256()
                {
                    return new HashLib.Crypto.SHA3.Blake256();
                }

                public static IHash CreateBlake384()
                {
                    return new HashLib.Crypto.SHA3.Blake384();
                }

                public static IHash CreateBlake512()
                {
                    return new HashLib.Crypto.SHA3.Blake512();
                }

                public static IHash CreateBlake(HashLib.HashSize a_hashSize)
                {
                    switch (a_hashSize)
                    {
                        case HashLib.HashSize.HashSize224: return CreateBlake224();
                        case HashLib.HashSize.HashSize256: return CreateBlake256();
                        case HashLib.HashSize.HashSize384: return CreateBlake384();
                        case HashLib.HashSize.HashSize512: return CreateBlake512();
                        default: throw new ArgumentException();
                    }
                    
                }

                public static IHash CreateBlueMidnightWish224()
                {
                    return new HashLib.Crypto.SHA3.BlueMidnightWish224();
                }

                public static IHash CreateBlueMidnightWish256()
                {
                    return new HashLib.Crypto.SHA3.BlueMidnightWish256();
                }

                public static IHash CreateBlueMidnightWish384()
                {
                    return new HashLib.Crypto.SHA3.BlueMidnightWish384();
                }

                public static IHash CreateBlueMidnightWish512()
                {
                    return new HashLib.Crypto.SHA3.BlueMidnightWish512();
                }

                public static IHash CreateBlueMidnightWish(HashLib.HashSize a_hashSize)
                {
                    switch (a_hashSize)
                    {
                        case HashLib.HashSize.HashSize224: return CreateBlueMidnightWish224();
                        case HashLib.HashSize.HashSize256: return CreateBlueMidnightWish256();
                        case HashLib.HashSize.HashSize384: return CreateBlueMidnightWish384();
                        case HashLib.HashSize.HashSize512: return CreateBlueMidnightWish512();
                        default: throw new ArgumentException();
                    }
                }

                public static IHash CreateCubeHash224()
                {
                    return new HashLib.Crypto.SHA3.CubeHash224();
                }

                public static IHash CreateCubeHash256()
                {
                    return new HashLib.Crypto.SHA3.CubeHash256();
                }

                public static IHash CreateCubeHash384()
                {
                    return new HashLib.Crypto.SHA3.CubeHash384();
                }

                public static IHash CreateCubeHash512()
                {
                    return new HashLib.Crypto.SHA3.CubeHash512();
                }

                public static IHash CreateCubeHash(HashLib.HashSize a_hashSize, int a_rounds = 16, int a_blockSize = 32)
                {
                    if (a_blockSize < 1)
                        throw new ArgumentOutOfRangeException ();
                    if (a_blockSize > 128)
                        throw new ArgumentOutOfRangeException();
                    if (a_rounds < 1)
                        throw new ArgumentOutOfRangeException();

                    if ((a_rounds == 16) && (a_blockSize == 32))
                    {
                        switch (a_hashSize)
                        {
                            case HashLib.HashSize.HashSize224: return CreateCubeHash224();
                            case HashLib.HashSize.HashSize256: return CreateCubeHash256();
                            case HashLib.HashSize.HashSize384: return CreateCubeHash384();
                            case HashLib.HashSize.HashSize512: return CreateCubeHash512();
                            default: throw new ArgumentException();
                        }
                    }
                    else
                    {
                        switch (a_hashSize)
                        {
                            case HashLib.HashSize.HashSize224: return new HashLib.Crypto.SHA3.CubeHashCustom(a_hashSize, a_rounds, a_blockSize);
                            case HashLib.HashSize.HashSize256: return new HashLib.Crypto.SHA3.CubeHashCustom(a_hashSize, a_rounds, a_blockSize);
                            case HashLib.HashSize.HashSize384: return new HashLib.Crypto.SHA3.CubeHashCustom(a_hashSize, a_rounds, a_blockSize);
                            case HashLib.HashSize.HashSize512: return new HashLib.Crypto.SHA3.CubeHashCustom(a_hashSize, a_rounds, a_blockSize);
                            default: throw new ArgumentException();
                        }
                    }
                }

                public static IHash CreateEcho224()
                {
                    return new HashLib.Crypto.SHA3.Echo224();
                }

                public static IHash CreateEcho256()
                {
                    return new HashLib.Crypto.SHA3.Echo256();
                }

                public static IHash CreateEcho384()
                {
                    return new HashLib.Crypto.SHA3.Echo384();
                }

                public static IHash CreateEcho512()
                {
                    return new HashLib.Crypto.SHA3.Echo512();
                }

                public static IHash CreateEcho(HashLib.HashSize a_hashSize)
                {
                    switch (a_hashSize)
                    {
                        case HashLib.HashSize.HashSize224: return CreateEcho224();
                        case HashLib.HashSize.HashSize256: return CreateEcho256();
                        case HashLib.HashSize.HashSize384: return CreateEcho384();
                        case HashLib.HashSize.HashSize512: return CreateEcho512();
                        default: throw new ArgumentException();
                    }
                }

                public static IHash CreateFugue224()
                {
                    return new HashLib.Crypto.SHA3.Fugue224();
                }

                public static IHash CreateFugue256()
                {
                    return new HashLib.Crypto.SHA3.Fugue256();
                }

                public static IHash CreateFugue384()
                {
                    return new HashLib.Crypto.SHA3.Fugue384();
                }

                public static IHash CreateFugue512()
                {
                    return new HashLib.Crypto.SHA3.Fugue512();
                }

                public static IHash CreateFugue(HashLib.HashSize a_hashSize)
                {
                    switch (a_hashSize)
                    {
                        case HashLib.HashSize.HashSize224: return CreateFugue224();
                        case HashLib.HashSize.HashSize256: return CreateFugue256();
                        case HashLib.HashSize.HashSize384: return CreateFugue384();
                        case HashLib.HashSize.HashSize512: return CreateFugue512();
                        default: throw new ArgumentException();
                    }
                }

                public static IHash CreateGroestl224()
                {
                    return new HashLib.Crypto.SHA3.Groestl224();
                }

                public static IHash CreateGroestl256()
                {
                    return new HashLib.Crypto.SHA3.Groestl256();
                }

                public static IHash CreateGroestl384()
                {
                    return new HashLib.Crypto.SHA3.Groestl384();
                }

                public static IHash CreateGroestl512()
                {
                    return new HashLib.Crypto.SHA3.Groestl512();
                }

                public static IHash CreateGroestl(HashLib.HashSize a_hashSize)
                {
                    switch (a_hashSize)
                    {
                        case HashLib.HashSize.HashSize224: return CreateGroestl224();
                        case HashLib.HashSize.HashSize256: return CreateGroestl256();
                        case HashLib.HashSize.HashSize384: return CreateGroestl384();
                        case HashLib.HashSize.HashSize512: return CreateGroestl512();
                        default: throw new ArgumentException();
                    }
                }

                public static IHash CreateHamsi224()
                {
                    return new HashLib.Crypto.SHA3.Hamsi224();
                }

                public static IHash CreateHamsi256()
                {
                    return new HashLib.Crypto.SHA3.Hamsi256();
                }

                public static IHash CreateHamsi384()
                {
                    return new HashLib.Crypto.SHA3.Hamsi384();
                }

                public static IHash CreateHamsi512()
                {
                    return new HashLib.Crypto.SHA3.Hamsi512();
                }

                public static IHash CreateHamsi(HashLib.HashSize a_hashSize)
                {
                    switch (a_hashSize)
                    {
                        case HashLib.HashSize.HashSize224: return CreateHamsi224();
                        case HashLib.HashSize.HashSize256: return CreateHamsi256();
                        case HashLib.HashSize.HashSize384: return CreateHamsi384();
                        case HashLib.HashSize.HashSize512: return CreateHamsi512();
                        default: throw new ArgumentException();
                    }
                }

                //

                public static IHash CreateKeccak224()
                {
                    return new HashLib.Crypto.SHA3.Keccak224();
                }

                public static IHash CreateKeccak256()
                {
                    return new HashLib.Crypto.SHA3.Keccak256();
                }

                public static IHash CreateKeccak384()
                {
                    return new HashLib.Crypto.SHA3.Keccak384();
                }

                public static IHash CreateKeccak512()
                {
                    return new HashLib.Crypto.SHA3.Keccak512();
                }

                public static IHash CreateKeccak(HashLib.HashSize a_hashSize)
                {
                    switch (a_hashSize)
                    {
                        case HashLib.HashSize.HashSize224: return CreateKeccak224();
                        case HashLib.HashSize.HashSize256: return CreateKeccak256();
                        case HashLib.HashSize.HashSize384: return CreateKeccak384();
                        case HashLib.HashSize.HashSize512: return CreateKeccak512();
                        default: throw new ArgumentException();
                    }
                }
            }

            public static class BuildIn
            {
                public static IHash CreateMD5CryptoServiceProvider()
                {
                    return new HashLib.Crypto.BuildIn.MD5CryptoServiceProvider();
                }

                public static IHash CreateRIPEMD160Managed()
                {
                    return new HashLib.Crypto.BuildIn.RIPEMD160Managed();
                }

                public static IHash CreateSHA1Cng()
                {
                    return new HashLib.Crypto.BuildIn.SHA1Cng();
                }

                public static IHash CreateSHA1CryptoServiceProvider()
                {
                    return new HashLib.Crypto.BuildIn.SHA1CryptoServiceProvider();
                }

                public static IHash CreateSHA1Managed()
                {
                    return new HashLib.Crypto.BuildIn.SHA1Managed();
                }

                public static IHash CreateSHA256Cng()
                {
                    return new HashLib.Crypto.BuildIn.SHA256Cng();
                }

                public static IHash CreateSHA256CryptoServiceProvider()
                {
                    return new HashLib.Crypto.BuildIn.SHA256CryptoServiceProvider();
                }

                public static IHash CreateSHA256Managed()
                {
                    return new HashLib.Crypto.BuildIn.SHA256Managed();
                }

                public static IHash CreateSHA384Cng()
                {
                    return new HashLib.Crypto.BuildIn.SHA384Cng();
                }

                public static IHash CreateSHA384CryptoServiceProvider()
                {
                    return new HashLib.Crypto.BuildIn.SHA384CryptoServiceProvider();
                }

                public static IHash CreateSHA384Managed()
                {
                    return new HashLib.Crypto.BuildIn.SHA384Managed();
                }

                public static IHash CreateSHA512Cng()
                {
                    return new HashLib.Crypto.BuildIn.SHA512Cng();
                }

                public static IHash CreateSHA512CryptoServiceProvider()
                {
                    return new HashLib.Crypto.BuildIn.SHA512CryptoServiceProvider();
                }

                public static IHash CreateSHA512Managed()
                {
                    return new HashLib.Crypto.BuildIn.SHA512Managed();
                }
            }

            public static IHash CreateGost()
            {
                return new HashLib.Crypto.Gost();
            }

            public static IHash CreateHAS160()
            {
                return new HashLib.Crypto.HAS160();
            }

            public static IHash CreateHaval_3_128()
            {
                return new HashLib.Crypto.Haval_3_128();
            }

            public static IHash CreateHaval_4_128()
            {
                return new HashLib.Crypto.Haval_4_128();
            }

            public static IHash CreateHaval_5_128()
            {
                return new HashLib.Crypto.Haval_5_128();
            }

            public static IHash CreateHaval_3_160()
            {
                return new HashLib.Crypto.Haval_3_160();
            }

            public static IHash CreateHaval_4_160()
            {
                return new HashLib.Crypto.Haval_4_160();
            }

            public static IHash CreateHaval_5_160()
            {
                return new HashLib.Crypto.Haval_5_160();
            }

            public static IHash CreateHaval_3_192()
            {
                return new HashLib.Crypto.Haval_3_192();
            }

            public static IHash CreateHaval_4_192()
            {
                return new HashLib.Crypto.Haval_4_192();
            }

            public static IHash CreateHaval_5_192()
            {
                return new HashLib.Crypto.Haval_5_192();
            }

            public static IHash CreateHaval_3_224()
            {
                return new HashLib.Crypto.Haval_3_224();
            }

            public static IHash CreateHaval_4_224()
            {
                return new HashLib.Crypto.Haval_4_224();
            }

            public static IHash CreateHaval_5_224()
            {
                return new HashLib.Crypto.Haval_5_224();
            }

            public static IHash CreateHaval_3_256()
            {
                return new HashLib.Crypto.Haval_3_256();
            }

            public static IHash CreateHaval_4_256()
            {
                return new HashLib.Crypto.Haval_4_256();
            }

            public static IHash CreateHaval_5_256()
            {
                return new HashLib.Crypto.Haval_5_256();
            }

            public static IHash CreateHaval(int a_rounds, HashLib.HashSize a_hashSize)
            {
                switch (a_rounds)
                {
                    case 3:

                        switch (a_hashSize)
                        {
                            case HashLib.HashSize.HashSize128: return CreateHaval_3_128();
                            case HashLib.HashSize.HashSize160: return CreateHaval_3_160();
                            case HashLib.HashSize.HashSize192: return CreateHaval_3_192();
                            case HashLib.HashSize.HashSize224: return CreateHaval_3_224();
                            case HashLib.HashSize.HashSize256: return CreateHaval_3_256();
                            default: throw new ArgumentException();
                        }

                    case 4:

                        switch (a_hashSize)
                        {
                            case HashLib.HashSize.HashSize128: return CreateHaval_4_128();
                            case HashLib.HashSize.HashSize160: return CreateHaval_4_160();
                            case HashLib.HashSize.HashSize192: return CreateHaval_4_192();
                            case HashLib.HashSize.HashSize224: return CreateHaval_4_224();
                            case HashLib.HashSize.HashSize256: return CreateHaval_4_256();
                            default: throw new ArgumentException();
                        }

                    case 5:

                        switch (a_hashSize)
                        {
                            case HashLib.HashSize.HashSize128: return CreateHaval_5_128();
                            case HashLib.HashSize.HashSize160: return CreateHaval_5_160();
                            case HashLib.HashSize.HashSize192: return CreateHaval_5_192();
                            case HashLib.HashSize.HashSize224: return CreateHaval_5_224();
                            case HashLib.HashSize.HashSize256: return CreateHaval_5_256();
                            default: throw new ArgumentException();
                        }

                    default: throw new ArgumentException();
                }
            }

            public static IHash CreateMD2()
            {
                return new HashLib.Crypto.MD2();
            }

            public static IHash CreateMD4()
            {
                return new HashLib.Crypto.MD4();
            }

            public static IHash CreateMD5()
            {
                return new HashLib.Crypto.MD5();
            }

            public static IHash CreateRIPEMD128()
            {
                return new HashLib.Crypto.RIPEMD128();
            }

            public static IHash CreateRIPEMD160()
            {
                return new HashLib.Crypto.RIPEMD160();
            }

            public static IHash CreateRIPEMD256()
            {
                return new HashLib.Crypto.RIPEMD256();
            }

            public static IHash CreateRIPEMD320()
            {
                return new HashLib.Crypto.RIPEMD320();
            }

            public static IHash CreateSHA1()
            {
                return new HashLib.Crypto.SHA1();
            }

            public static IHash CreateSHA224()
            {
                return new HashLib.Crypto.SHA224();
            }

            public static IHash CreateSHA256()
            {
                return new HashLib.Crypto.SHA256();
            }

            public static IHash CreateSHA384()
            {
                return new HashLib.Crypto.SHA384();
            }

            public static IHash CreateSHA512()
            {
                return new HashLib.Crypto.SHA512();
            }

            public static IHash CreateSnefru_4_128()
            {
                return new HashLib.Crypto.Snefru_4_128();
            }

            public static IHash CreateSnefru_4_256()
            {
                return new HashLib.Crypto.Snefru_4_256();
            }

            public static IHash CreateSnefru_8_128()
            {
                return new HashLib.Crypto.Snefru_8_128();
            }

            public static IHash CreateSnefru_8_256()
            {
                return new HashLib.Crypto.Snefru_8_256();
            }

            public static IHash CreateSnefru(int a_rounds, HashLib.HashSize a_hashSize)
            {
                switch (a_rounds)
                {
                    case 4:

                        switch (a_hashSize)
                        {
                            case HashLib.HashSize.HashSize128: return CreateSnefru_4_128();
                            case HashLib.HashSize.HashSize256: return CreateSnefru_4_256();
                            default: throw new ArgumentException();
                        }

                    case 8:

                        switch (a_hashSize)
                        {
                            case HashLib.HashSize.HashSize128: return CreateSnefru_8_128();
                            case HashLib.HashSize.HashSize256: return CreateSnefru_8_256();
                            default: throw new ArgumentException();
                        }

                    default: throw new ArgumentException();
                }
            }

            public static IHash CreateTiger_3_192()
            {
                return new HashLib.Crypto.Tiger_3_192();
            }

            public static IHash CreateTiger_4_192()
            {
                return new HashLib.Crypto.Tiger_4_192();
            }

            public static IHash CreateTiger(int a_rounds)
            {
                switch (a_rounds)
                {
                    case 3: return CreateTiger_3_192();
                    case 4: return CreateTiger_4_192();
                    default: throw new ArgumentException();
                }
            }

            public static IHash CreateTiger2()
            {
                return new HashLib.Crypto.Tiger2();
            }

            public static IHash CreateWhirlpool()
            {
                return new HashLib.Crypto.Whirlpool();
            }
        }

        public static class HMAC
        {
            public static IHMAC CreateHMAC(IHash a_hash)
            {
                if (a_hash is IHMAC)
                    return (IHMAC)a_hash;
                else if (a_hash is IHasHMACBuildIn)
                {
                    HashCryptoBuildIn h = (HashCryptoBuildIn)a_hash;
                    return new HMACBuildInAdapter(h.GetBuildHMAC(), h.BlockSize);
                }

                return new HMACNotBuildInAdapter(a_hash);
            }
        }

        public static class Wrappers
        {
            public static System.Security.Cryptography.HashAlgorithm HashToHashAlgorithm(IHash a_hash)
            {
                return new HashAlgorithmWrapper(a_hash);
            }

            public static IHash HashAlgorithmToHash(System.Security.Cryptography.HashAlgorithm a_hash, int a_blockSize = -1)
            {
                return new HashCryptoBuildIn(a_hash, a_blockSize);
            }
        }
    }
}
