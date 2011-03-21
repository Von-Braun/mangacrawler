using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    /// <summary>
    /// Main diffrence from SortedList. Keys may repeats.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    internal class OrderedList<K, V>
    {
        private SortedList<K, List<V>> m_list = new SortedList<K, List<V>>();

        public void Add(K a_key, V a_value)
        {
            if (Values.Contains(a_value))
                throw new Exception();

            if (!Keys.Contains(a_key))
                m_list.Add(a_key, new List<V>());

            m_list[a_key].Add(a_value);
        }

        public IEnumerable<V> Values
        {
            get
            {
                foreach (var sublist in m_list.Values)
                    foreach (var v in sublist)
                        yield return v;
            }
        }

        public IEnumerable<K> Keys
        {
            get
            {
                return m_list.Keys;
            }
        }

        public void RemoveByValue(V a_value)
        {
            foreach (var sublist in m_list.Values)
            {
                if (sublist.Remove(a_value))
                    break;
            }
        }

        public int Count 
        {
            get
            {
                return Values.Count();
            }
        }

        public V RemoveFirst()
        {
            V v = Values.First();
            RemoveByValue(v);
            return v;
        }
    }
}
