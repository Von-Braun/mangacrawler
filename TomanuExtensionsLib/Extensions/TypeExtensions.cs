using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace TomanuExtensions
{
    [DebuggerStepThrough]
    public static class TypeExtensions
    {
        public static bool IsDerivedFrom(this Type a_type, Type a_baseType)
        {
            Debug.Assert(a_type != null);
            Debug.Assert(a_baseType != null);
            Debug.Assert(a_type.IsClass);
            Debug.Assert(a_baseType.IsClass);

            return a_baseType.IsAssignableFrom(a_type);
        }

        public static bool IsImplementInterface(this Type a_type, Type a_interfaceType)
        {
            Debug.Assert(a_type != null);
            Debug.Assert(a_interfaceType != null);
            Debug.Assert(a_type.IsClass || a_type.IsInterface || a_type.IsValueType);
            Debug.Assert(a_interfaceType.IsInterface);

            return a_interfaceType.IsAssignableFrom(a_type);
        }

        public static IEnumerable<Type> GetBaseTypes(this Type a_type, 
            bool a_with_this = false)
        {
            if (a_with_this)
                yield return a_type;

            Type t = a_type;

            while (t.BaseType != null)
            {
                t = t.BaseType;
                yield return t;
            }
        }

        public static string GetParentFullName(this Type a_type)
        {
            return Path.GetFileNameWithoutExtension(a_type.FullName);
        }

        /// <summary>
        /// Get all private, protected, public properties from this type and sub-types. 
        /// Without abstract properties. In case of overriding return top-most one.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            foreach (var t in type.GetBaseTypes(true))
            {
                if (t == typeof(Object))
                    break;
                if (t == typeof(ValueType))
                    break;

                PropertyInfo[] type_props = t.GetProperties(
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance);

                foreach (var poss_prop in type_props.Reverse())
                {
                    if (poss_prop.IsAbstract())
                        continue;

                    if (result.All(prop => !prop.IsDerivedFrom(poss_prop, true)))
                        result.Add(poss_prop);
                }
            }

            result.Reverse();
            return result;
        }
    }
}
