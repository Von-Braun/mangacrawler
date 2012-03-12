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
        protected Object m_load_from_xml_lock = new Object();

        internal void ReplaceInnerCollection<K>(IEnumerable<T> a_new, bool a_merge, Func<T, K> a_key_selector) 
        {
            ReplaceInnerCollection(a_new, a_merge, a_key_selector, out Changed);
        }

        internal void ReplaceInnerCollection<K>(IEnumerable<T> a_new, bool a_merge, Func<T, K> a_key_selector, 
            out bool a_changed)
        {
            if (a_merge)
                m_list = MergeAndRemoveOrphans(m_list, a_new, a_key_selector, out a_changed);
            else
            {
                m_list = a_new.ToList();
                a_changed = true;
            }
        }

        internal static List<T> MergeAndRemoveOrphans<K>(IList<T> a_list,
            IEnumerable<T> a_new, Func<T, K> a_key_selector)
        {
            bool changed;
            return MergeAndRemoveOrphans(a_list, a_new, a_key_selector, out changed);
        }

        internal static List<T> MergeAndRemoveOrphans<K>(IList<T> a_list,
            IEnumerable<T> a_new, Func<T, K> a_key_selector, out bool a_changed)
        {
            var dict = a_list.ToDictionary(a_key_selector);
            var result = a_new.ToList();

            int replaced = 0;

            for (int i = 0; i < result.Count; i++)
            {
                K key = a_key_selector(result[i]);
                if (dict.ContainsKey(key))
                {
                    result[i] = dict[key];
                    dict.Remove(key);
                    replaced++;
                }
            }

            if (a_list.Count != result.Count)
                a_changed = true;
            else
                a_changed = replaced != a_list.Count;

            foreach (var orphan in dict.Values)
                orphan.RemoveOrphan();

            return result;
        }

        internal bool LoadedFromXml
        {
            get
            {
                lock (m_load_from_xml_lock)
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

        protected abstract void EnsureLoaded();
        internal abstract void ClearCache();
    }
}
