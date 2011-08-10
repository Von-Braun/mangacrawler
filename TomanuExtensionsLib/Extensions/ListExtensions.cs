using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomanuExtensions
{
    public static class ListExtensions
    {
        public static void RemoveLast<T>(this List<T> a_list)
        {
            a_list.RemoveAt(a_list.Count - 1);
        }

        public static void RemoveRange<T>(this IList<T> a_list, IEnumerable<T> a_elements)
        {
            foreach (var ele in a_elements)
                a_list.Remove(ele);
        }

        public static int GetHashCode<T>(IList<T> a_list)
        {
            int hash = 0;

            foreach (var el in a_list)
                hash ^= el.GetHashCode();

            return hash;
        }
    }
}
