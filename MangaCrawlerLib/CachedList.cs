using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    /// <summary>
    /// Thread-safe, copy on write semantic.
    /// </summary>
    /// <param name="series"></param>
    internal abstract class CachedList<T> : IList<T> where T : Entity
    {
        protected bool m_loaded_from_xml;
        protected List<T> m_list;
        protected Object m_lock = new Object();

        internal void ReplaceInnerCollection(IEnumerable<T> a_new) 
        {
            EnsureLoaded();

            m_list = a_new.ToList();
        }

        internal void ReplaceInnerCollection<K>(IEnumerable<T> a_new, Func<T, K> a_key_selector)
        {
            EnsureLoaded();

            lock (m_lock)
            {
                m_list = Merge(a_new, m_list, a_key_selector);
                Remove(m_list, a_new, a_key_selector);
            }
        }

        internal static void Remove<K>(IList<T> a_list,
            IEnumerable<T> a_new, Func<T, K> a_key_selector)
        {
            var dict = a_new.ToDictionary(a_key_selector);

            for (int i = a_list.Count - 1; i >= 0; i--)
            {
                K key = a_key_selector(a_list[i]);
                if (!dict.ContainsKey(key))
                    a_list.RemoveAt(i);
            }
        }

        internal static List<T> Merge<K>(IEnumerable<T> a_new, IEnumerable<T> a_prev, Func<T, K> a_key_selector)
        {
            IDictionary<K, T> dict = a_prev.ToDictionary(a_key_selector);

            var result = a_new.ToList();

            for (int i = 0; i < result.Count; i++)
            {
                K key = a_key_selector(result[i]);
                if (dict.ContainsKey(key))
                    result[i] = dict[key];
            }

            return result;
        }

        internal bool Filled
        {
            get
            {
                return (m_list != null);
            }
        }

        internal bool LoadedFromXml
        {
            get
            {
                EnsureLoaded();

                lock (m_lock)
                {
                    return m_loaded_from_xml;
                }
            }
        }

        public int IndexOf(T a_item)
        {
            EnsureLoaded();

            return m_list.IndexOf(a_item);
        }

        public void Insert(int a_index, T a_item)
        {
            throw new InvalidOperationException();
        }

        public void RemoveAt(int a_index)
        {
            throw new InvalidOperationException();
        }

        public T this[int a_index]
        {
            get
            {
                EnsureLoaded();

                return m_list[a_index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(T a_item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T a_item)
        {
            EnsureLoaded();

            return m_list.Contains(a_item);
        }

        public void CopyTo(T[] a_array, int a_array_index)
        {
            EnsureLoaded();

            m_list.CopyTo(a_array, a_array_index);
        }

        public int Count
        {
            get
            {
                EnsureLoaded();

                return m_list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            EnsureLoaded();

            return m_list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            EnsureLoaded();

            return m_list.GetEnumerator();
        }

        public override string ToString()
        {
            var list = m_list;
            
            if (list == null)
                return "Uninitialized";
            else
                return String.Format("Count: {0}, LoadedFromXml: {1}", list.Count, LoadedFromXml);
        }

        protected abstract void EnsureLoaded();
    }
}
