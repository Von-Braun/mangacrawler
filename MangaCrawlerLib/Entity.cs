using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    public abstract class Entity
    {
        public ulong ID { get; internal set; }
        public string URL { get; protected set; }

        protected Entity(ulong a_id)
        {
            ID = a_id;
        }

        internal static List<T> MergeAndRemoveOrphans<T, K>(IList<T> a_list, IEnumerable<T> a_new, 
            Func<T, K> a_key_selector) where T : Entity
        {
            bool changed;
            return MergeAndRemoveOrphans(a_list, a_new, a_key_selector, out changed);
        }

        internal static List<T> MergeAndRemoveOrphans<T, K>(IList<T> a_list, 
            IEnumerable<T> a_new, Func<T, K> a_key_selector, out bool a_changed) where T : Entity
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

        internal abstract Crawler Crawler { get; }
        protected internal abstract void RemoveOrphan();
    }
}
