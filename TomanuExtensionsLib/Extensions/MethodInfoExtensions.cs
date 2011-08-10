using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TomanuExtensions
{
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// With virtual keyword.
        /// </summary>
        /// <param name="a_pi"></param>
        /// <returns></returns>
        public static bool IsVirtual(this MethodInfo a_mi)
        {
            if (!a_mi.IsVirtual)
                return false;
            if (a_mi.IsAbstract)
                return false;
            if (a_mi.GetBaseDefinition() == null)
                return true;
            if (a_mi == a_mi.GetBaseDefinition())
                return true;

            return false;
        }

        /// <summary>
        /// With abstract keyword.
        /// </summary>
        /// <param name="a_pi"></param>
        /// <returns></returns>
        public static bool IsAbstract(this MethodInfo a_mi)
        {
            return a_mi.IsAbstract;
        }

        /// <summary>
        /// With override keyword.
        /// </summary>
        /// <param name="a_pi"></param>
        /// <returns></returns>
        public static bool IsOverriden(this MethodInfo a_mi)
        {
            if (!a_mi.IsVirtual)
                return false;
            if (a_mi.IsAbstract)
                return false;
            if (a_mi.GetBaseDefinition() == null)
                return false;
            if (a_mi == a_mi.GetBaseDefinition())
                return false;

            return true;
        }

        public static IEnumerable<MethodInfo> GetBaseDefinitions(this MethodInfo a_mi, 
            bool a_with_this = false)
        {
            if (a_with_this)
                yield return a_mi;

            MethodInfo t = a_mi;

            while ((t.GetBaseDefinition() != null) && (t.GetBaseDefinition() != t))
            {
                t = t.GetBaseDefinition();
                yield return t;
            }
        }

        /// <summary>
        /// With no special keyword.
        /// </summary>
        /// <param name="a_pi"></param>
        /// <returns></returns>
        public static bool IsNormal(this MethodInfo a_mi)
        {
            return !a_mi.IsVirtual && !a_mi.IsAbstract;
        }
    }
}
