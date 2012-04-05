using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomanuExtensions;

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

            var list = m_list.ToList();

            foreach (var el in m_list.Except(a_new))
                list.Remove(el);

            int index = 0;
            foreach (var el in a_new)
            {
                if (list.Count == index)
                    list.Insert(index, el);
                if (list[index] != el)
                    list.Insert(index, el);
                index++;
            }

            m_list = list;
        }

        internal void ReplaceInnerCollection(IEnumerable<T> a_new, bool a_remove, Func<T, string> a_key_selector)
        {
            EnsureLoaded();

            var copy = m_list.ToList();
            var new_list = a_new.ToList();

            Merge(new_list, copy, a_key_selector);

            if (a_remove)
            {
                var to_remove = copy.Except(new_list).ToList();
                new_list.RemoveAll(el => to_remove.Contains(el));
            }

            m_list = new_list;
        }

        private static void Merge(List<T> a_new, List<T> a_local,
            Func<T, string> a_key_selector)
        {
            var dups = a_local.Select(a_key_selector).ExceptExact(
                a_local.Select(a_key_selector).Distinct());
            IDictionary<string, T> local_dict = a_local.ToDictionary(a_key_selector);

            for (int i = 0; i < a_new.Count; i++)
            {
                string key = a_key_selector(a_new[i]);
                if (local_dict.ContainsKey(key))
                    a_new[i] = local_dict[key];
            }
        }

        internal bool Filled
        {
            get
            {
                return (m_list != null);
            }
        }

        internal bool IsLoadedFromXml
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
                return String.Format("Count: {0}, LoadedFromXml: {1}", list.Count, IsLoadedFromXml);
        }

        protected abstract void EnsureLoaded();
    }
}
