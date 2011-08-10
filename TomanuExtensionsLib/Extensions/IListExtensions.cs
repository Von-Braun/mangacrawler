using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TomanuExtensions
{
    public static class IListExtensions
    {
        public static int IndexOf<T>(this IList<T> a_list, T a_element)
        {
            for (int i = 0; i < a_list.Count; i++)
            {
                if (a_element.Equals(a_list[i]))
                    return i;
            }

            return -1;
        }

        public static int IndexOf<T>(this IList<T> a_list, T a_element, IEqualityComparer<T> a_comparer)
        {
            for (int i = 0; i < a_list.Count; i++)
            {
                if (a_comparer.Equals(a_list[i], a_element))
                    return i;
            }

            return -1;
        }

        public static void RemoveLast<T>(this IList<T> a_list)
        {
            a_list.RemoveAt(a_list.Count - 1);
        }

        public static T Last<T>(this IList<T> a_list)
        {
            return a_list[a_list.Count - 1];
        }
    }
}
