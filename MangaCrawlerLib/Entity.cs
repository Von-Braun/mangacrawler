using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    public abstract class Entity
    {
        private static int IDCounter = 0;

        public int ID { get; protected set; }
        public string URL { get; protected set; }

        internal abstract Crawler Crawler { get; }

        internal Entity()
        {
            IDCounter++;
            ID = IDCounter;
        }

        protected static void Sync<T, K>(IEnumerable<T> a_new, List<T> a_list, Func<T, K> a_key_selector,
            bool a_remove, out IList<T> a_added, out IList<T> a_removed) where K : IEquatable<K>
        {
            IDictionary<K, T> new_pages_dict = a_new.ToDictionary<T, K>(a_key_selector);
            IDictionary<K, T> pages_dict = a_list.ToDictionary(a_key_selector);

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
                a_list.RemoveRange(a_removed);
                pages_dict.RemoveRange(to_remove);
            }

            a_added = new List<T>();

            int index = 0;
            foreach (var el in a_new)
            {
                if (a_list.Count <= index)
                {
                    a_list.Insert(index, el);
                    a_added.Add(el);
                }
                else
                {
                    var pr = a_list[index];

                    if (!a_key_selector(pr).Equals(a_key_selector(el)))
                    {
                        a_list.Insert(index, el);
                        a_added.Add(el);
                    }
                }
                index++;
            }
        }
    }
}
