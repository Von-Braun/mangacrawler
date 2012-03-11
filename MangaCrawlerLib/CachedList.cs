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
    internal abstract class CachedList<T> : IList<T>
    {
        public bool Changed;
        protected bool m_loaded_from_xml;
        protected List<T> m_list;
        protected Object m_load_from_xml_lock = new Object();

        internal void ReplaceInnerCollection(List<T> a_new)
        {
            Changed = true;
            m_list = a_new;
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
