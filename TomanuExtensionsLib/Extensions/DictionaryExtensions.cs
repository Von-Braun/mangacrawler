using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TomanuExtensions
{
    [DebuggerStepThrough]
    public static class DictionaryExtensions
    {
        public static IDictionary<V, K> Invert<K, V>(
            this IDictionary<K, V> a_dictionary)
        {
            return a_dictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
        }

        public static void RemoveRange<V, K>(
            this IDictionary<K, V> a_dictionary, IEnumerable<K> a_keys)
        {
            foreach (var key in a_keys)
                a_dictionary.Remove(key);
        }
    }
}