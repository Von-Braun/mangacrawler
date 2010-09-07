using System;
using System.Reflection;
using System.Linq;
using System.Collections.ObjectModel;

namespace HashLib
{
    public static class HashFactory
    {
        public static class Crypto
        {
            public static IHash CreateSHA512()
            {
                return new HashLib.SHA512();
            }
        }
    }
}
