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
        public bool Changed;
        protected bool m_loaded_from_xml;
        protected List<T> m_list;
        protected Object m_lock = new Object();

        internal void ReplaceInnerCollection<K>(IEnumerable<T> a_new, IDictionary<K, T> a_prev, 
            bool a_all_downloaded, Func<T, K> a_key_selector) 
        {
            EnsureLoaded();

            lock (m_lock)
            {
                m_list = Merge(a_new, a_prev, a_key_selector);

                if (a_all_downloaded)
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

        internal static List<T> Merge<K>(IEnumerable<T> a_new, IDictionary<K, T> a_prev, Func<T, K> a_key_selector)
        {
            var result = a_new.ToList();

            for (int i = 0; i < result.Count; i++)
            {
                K key = a_key_selector(result[i]);
                if (a_prev.ContainsKey(key))
                    result[i] = a_prev[key];
            }

            return result;
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
            if (m_list == null)
                return "Uninitialized";
            else
                return String.Format("Count: {0}, LoadedFromXml: {1}", m_list.Count, LoadedFromXml);
        }

        protected abstract void EnsureLoaded();
    }
}
