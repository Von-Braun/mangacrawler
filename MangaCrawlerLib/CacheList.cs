using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomanuExtensions;
using System.Collections;

namespace MangaCrawlerLib
{
    public class CacheList<T> : IList<T>
    {
        private IList<T> m_underlist;
        private List<T> m_list = new List<T>();
        private Object m_underlist_parent;
        private volatile bool m_loaded = false;

        internal CacheList(Object a_underlist_parent, IList<T> a_underlist)
        {
            m_underlist_parent = a_underlist_parent;
            m_underlist = a_underlist;
        }

        public int IndexOf(T a_item)
        {
            EnsureLoaded();

            lock (m_list)
            {
                return m_list.IndexOf(a_item);
            }
        }

        public void Insert(int a_index, T a_item)
        {
            EnsureLoaded();

            lock (m_list)
            {
                m_underlist.Insert(a_index, a_item);
                m_list.Insert(a_index, a_item);
            }
        }

        public void RemoveAt(int a_index)
        {
            EnsureLoaded();

            lock (m_list)
            {
                m_underlist.RemoveAt(a_index);
                m_list.RemoveAt(a_index);
            }
        }

        public T this[int a_index]
        {
            get
            {
                EnsureLoaded();

                lock (m_list)
                {
                    return m_list[a_index];
                }
            }
            set
            {
                EnsureLoaded();

                lock (m_list)
                {
                    m_list[a_index] = value;
                    m_underlist[a_index] = value;
                }
            }
        }

        public void Add(T a_item)
        {
            EnsureLoaded();

            lock (m_list)
            {
                m_underlist.Add(a_item);
                m_list.Add(a_item);
            }
        }

        public void Clear()
        {
            lock (m_list)
            {
                m_underlist.Clear();
                m_list.Clear();
            }
        }

        public bool Contains(T a_item)
        {
            EnsureLoaded();

            lock (m_list)
            {
                return m_list.Contains(a_item);
            }
        }

        public void CopyTo(T[] a_array, int a_array_index)
        {
            EnsureLoaded();

            lock (m_list)
            {
                m_list.CopyTo(a_array, a_array_index);
            }
        }

        public int Count
        {
            get
            {
                EnsureLoaded();

                lock (m_list)
                {
                    return m_list.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(T a_item)
        {
            EnsureLoaded();

            lock (m_list)
            {
                m_underlist.Remove(a_item);
                return m_list.Remove(a_item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            EnsureLoaded();

            lock (m_list)
            {
                return m_list.ToList().GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            EnsureLoaded();

            lock (m_list)
            {
                return m_list.ToList().GetEnumerator();
            }
        }

        private void EnsureLoaded()
        {
            if (m_loaded)
                return;

            NH.TransactionLock(m_underlist_parent, () =>
            {
                lock (m_list)
                {
                    foreach (var el in m_underlist)
                        m_list.Add(el);
                }
            });

            m_loaded = true;
        }

        public void Sync<K>(IEnumerable<T> a_transient, Func<T, K> a_key_selector,
            bool a_remove, out IList<T> a_added, out IList<T> a_removed) where K : IEquatable<K>
        {
            IDictionary<K, T> new_pages_dict = a_transient.ToDictionary<T, K>(a_key_selector);
            IDictionary<K, T> pages_dict = this.ToDictionary(a_key_selector);

            a_removed = new List<T>();

            if (a_remove)
            {
                List<K> to_remove = new List<K>();

                foreach (var key in pages_dict.Keys)
                {
                    if (!new_pages_dict.Keys.Contains(key))
                        to_remove.Add(key);
                }

                a_removed = (from key in to_remove
                             select pages_dict[key]).ToList();
                this.RemoveRange(a_removed);
                pages_dict.RemoveRange(to_remove);
            }

            a_added = new List<T>();

            int index = 0;
            foreach (var tr in a_transient)
            {
                if (Count <= index)
                {
                    Insert(index, tr);
                    a_added.Add(tr);
                }
                else
                {
                    var pr = this[index];

                    if (!a_key_selector(pr).Equals(a_key_selector(tr)))
                    {
                        Insert(index, tr);
                        a_added.Add(tr);
                    }
                }
                index++;
            }
        }

        internal void ClearCache()
        {
            lock (m_list)
            {
                m_loaded = false;
                m_list.Clear();
            }
        }
    }
}
