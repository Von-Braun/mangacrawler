using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomanuExtensions
{
    public static class DictionaryExtensions
    {
        public static IDictionary<TValue, TKey> Invert<TKey, TValue>(
            this IDictionary<TKey, TValue> a_dictionary)
        {
            return a_dictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
        }
    }
}
