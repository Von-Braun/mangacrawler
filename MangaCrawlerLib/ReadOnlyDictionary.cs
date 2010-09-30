using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;

namespace MangaCrawlerLib
{
    public sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IDictionary<TKey, TValue> m_dictionary;

        public ReadOnlyDictionary(IDictionary<TKey, TValue> a_source)
        {
            m_dictionary = a_source;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in m_dictionary)
            {
                yield return item;
            }
        }

        public bool ContainsKey(TKey a_key)
        {
            return m_dictionary.ContainsKey(a_key);
        }

        public bool TryGetValue(TKey a_key, out TValue a_value)
        {
            return m_dictionary.TryGetValue(a_key, out a_value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> a_item)
        {
            return m_dictionary.Contains(a_item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] a_array, int a_arrayIndex)
        {
            m_dictionary.CopyTo(a_array, a_arrayIndex);
        }

        public TValue this[TKey a_key]
        {
            get
            {
                return m_dictionary[a_key];
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return new ReadOnlyCollection<TKey>(new List<TKey>(m_dictionary.Keys));
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return new ReadOnlyCollection<TValue>(new List<TValue>(m_dictionary.Values));
            }
        }

        public int Count
        {
            get
            {
                return m_dictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get 
            { 
                return true; 
            }
        }

        void IDictionary<TKey, TValue>.Add(TKey a_key, TValue a_value)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey a_key)
        {
            throw new NotSupportedException();
        }

        TValue IDictionary<TKey, TValue>.this[TKey a_key]
        {
            get
            {
                return this[a_key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> a_item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> a_item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
