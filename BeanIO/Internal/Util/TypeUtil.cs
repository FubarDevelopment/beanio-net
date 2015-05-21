using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BeanIO.Internal.Util
{
    public static class TypeUtil
    {
        /// <summary>
        /// Gets type <see cref="Type"/> for a given type name.
        /// </summary>
        /// <param name="typeName">The type name (<see cref="Type.AssemblyQualifiedName"/>).</param>
        /// <returns>The type for the given type name or null if it wasn't found.</returns>
        public static Type ToBeanType(this string typeName)
        {
            return Type.GetType(typeName, false);
        }

        public static bool IsInstanceOf(this Type testType, Type refType)
        {
            if (testType.IsConstructedGenericType)
            {
                testType = testType.GetGenericTypeDefinition();
            }

            var comparer = new TypeComparer();

            if (comparer.Compare(refType, testType) == 0)
                return true;

            var typeInfo = testType.GetTypeInfo();
            if (typeInfo.ImplementedInterfaces.Any(x => comparer.Compare(refType, x) == 0))
                return true;

            testType = typeInfo.BaseType;
            while (testType != null)
            {
                if (comparer.Compare(refType, testType) == 0)
                    return true;
                typeInfo = testType.GetTypeInfo();
                testType = typeInfo.BaseType;
            }

            return false;
        }

        public static string GetFullName(this Type t)
        {
            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(t.Namespace))
                result.AppendFormat("{0}.", t.Namespace);
            result.Append(t.Name);
            result.AppendFormat(", {0}", t.GetTypeInfo().Assembly.FullName);
            return result.ToString();
        }

        private class TypeComparer : IComparer<Type>
        {
            public int Compare(Type x, Type y)
            {
                if (ReferenceEquals(x, y))
                    return 0;
                if (x == null)
                    return y == null ? 0 : -1;
                if (y == null)
                    return 1;

                var hasMissingFullName = string.IsNullOrEmpty(x.AssemblyQualifiedName)
                                         || string.IsNullOrEmpty(y.AssemblyQualifiedName);
                var name1 = hasMissingFullName ? x.GetFullName() : x.AssemblyQualifiedName;
                var name2 = hasMissingFullName ? y.GetFullName() : y.AssemblyQualifiedName;
                return StringComparer.Ordinal.Compare(name1, name2);
            }
        }
    }
}
