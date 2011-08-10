using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TomanuExtensions
{
    public static class PropertyInfoExtensions
    {
        /// <summary>
        /// With virtual keyword.
        /// </summary>
        /// <param name="a_pi"></param>
        /// <returns></returns>
        public static bool IsVirtual(this PropertyInfo a_pi)
        {
            if (a_pi.GetAccessors(true).Length == 0)
                return false;

            return a_pi.GetAccessors(true)[0].IsVirtual();
        }

        /// <summary>
        /// With abstract keyword.
        /// </summary>
        /// <param name="a_pi"></param>
        /// <returns></returns>
        public static bool IsAbstract(this PropertyInfo a_pi)
        {
            if (a_pi.GetAccessors(true).Length == 0)
                return false;

            return a_pi.GetAccessors(true)[0].IsAbstract();
        }

        /// <summary>
        /// With override keyword.
        /// </summary>
        /// <param name="a_pi"></param>
        /// <returns></returns>
        public static bool IsOverriden(this PropertyInfo a_pi)
        {
            if (a_pi.GetAccessors(true).Length == 0)
                return false;

            return a_pi.GetAccessors(true)[0].IsOverriden();
        }

        /// <summary>
        /// With no special keyword.
        /// </summary>
        /// <param name="a_pi"></param>
        /// <returns></returns>
        public static bool IsNormal(this PropertyInfo a_pi)
        {
            if ((a_pi.GetGetMethod() == null) &&
                (a_pi.GetSetMethod() == null))
            {
                return true;
            }

            return a_pi.GetAccessors(true)[0].IsNormal();
        }

        public static bool IsDerivedFrom(this PropertyInfo a_super, PropertyInfo a_sub,
            bool a_with_this = false)
        {
            if (a_super.Name != a_sub.Name)
                return false;
            if (a_super.PropertyType != a_sub.PropertyType)
                return false;
            if (!a_super.GetIndexParameters().SequenceEqual(a_sub.GetIndexParameters()))
                return false;
            if (a_super.DeclaringType == a_sub.DeclaringType)
                return a_with_this;

            MethodInfo m1 = a_super.GetGetMethod(true);
            MethodInfo m3 = a_sub.GetGetMethod(true);

            if ((m1 != null) && (m3 != null))
            {
                if (m1.GetBaseDefinitions().ContainsAny(m3.GetBaseDefinitions(true)))
                    return true;
            }
            else if ((m1 != null) || (m3 != null))
                return false;

            MethodInfo m2 = a_super.GetSetMethod(true);
            MethodInfo m4 = a_sub.GetSetMethod(true);

            if ((m2 != null) && (m4 != null))
            {
                if (m2.GetBaseDefinitions().ContainsAny(m4.GetBaseDefinitions(true)))
                    return true;
            }
            else if ((m2 != null) || (m4 != null))
                return false;

            return false;
        }
    }
}
