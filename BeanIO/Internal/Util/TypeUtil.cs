using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// Type utility functions
    /// </summary>
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

        /// <summary>
        /// Is the <paramref name="testType"/> an instance of <paramref name="refType"/>?
        /// </summary>
        /// <param name="testType">The type to test for being an instance of <paramref name="refType"/></param>
        /// <param name="refType">The reference type to test against</param>
        /// <returns>true, if <paramref name="testType"/> is an instance of <paramref name="refType"/></returns>
        public static bool IsInstanceOf(this Type testType, Type refType)
        {
            if (testType.IsConstructedGenericType)
            {
                testType = testType.GetGenericTypeDefinition();
            }

            var refTypeInfo = refType.GetTypeInfo();
            var testTypeInfo = testType.GetTypeInfo();
            if (refTypeInfo.IsAssignableFrom(testTypeInfo))
                return true;

            var comparer = new TypeComparer();

            if (comparer.Compare(refType, testType) == 0)
                return true;

            if (testTypeInfo.ImplementedInterfaces.Any(x => comparer.Compare(refType, x) == 0))
                return true;

            testType = testTypeInfo.BaseType;
            while (testType != null)
            {
                if (comparer.Compare(refType, testType) == 0)
                    return true;
                testTypeInfo = testType.GetTypeInfo();
                if (refTypeInfo.IsAssignableFrom(testTypeInfo))
                    return true;
                testType = testTypeInfo.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Get the full name using the <see cref="Type.Namespace"/>, <see cref="Type.Name"/>, and <see cref="Assembly.FullName"/>
        /// </summary>
        /// <param name="t">The type to get the full name for</param>
        /// <returns>The full type name</returns>
        public static string GetFullName(this Type t)
        {
            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(t.Namespace))
                result.AppendFormat("{0}.", t.Namespace);
            result.Append(t.Name);
            result.AppendFormat(", {0}", t.GetTypeInfo().Assembly.FullName);
            return result.ToString();
        }

        /// <summary>
        /// The comparer that nullifies some .NET insanities where <see cref="Type.FullName"/> is null.
        /// </summary>
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
