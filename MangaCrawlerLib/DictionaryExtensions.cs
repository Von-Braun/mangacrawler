using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    public static class DictionaryExtensions
    {
        public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> a_source)
        {
            return new ReadOnlyDictionary<TKey, TValue>(a_source);
        }
    }
}
