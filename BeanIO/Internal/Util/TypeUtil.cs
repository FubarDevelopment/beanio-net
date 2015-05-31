using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using NodaTime;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// Type utility functions
    /// </summary>
    public static class TypeUtil
    {
        private static readonly IDictionary<string, Type> _wellKnownTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "string", typeof(string) },
                { "bool", typeof(bool) },
                { "boolean", typeof(bool) },
                { "byte", typeof(byte) },
                { "ubyte", typeof(byte) },
                { "sbyte", typeof(sbyte) },
                { "char", typeof(char) },
                { "character", typeof(char) },
                { "short", typeof(short) },
                { "Int16", typeof(short) },
                { "ushort", typeof(ushort) },
                { "UInt16", typeof(ushort) },
                { "int", typeof(int) },
                { "Integer", typeof(int) },
                { "Int32", typeof(int) },
                { "uint", typeof(uint) },
                { "Unsigned", typeof(uint) },
                { "UInt32", typeof(uint) },
                { "long", typeof(long) },
                { "Int64", typeof(long) },
                { "ulong", typeof(ulong) },
                { "UInt64", typeof(ulong) },
                { "float", typeof(float) },
                { "single", typeof(float) },
                { "double", typeof(double) },
                { "decimal", typeof(decimal) },
                { "BigDecimal", typeof(decimal) },
                { "uuid", typeof(Guid) },
                { "guid", typeof(Guid) },
                { "url", typeof(Uri) },
                { "uri", typeof(Uri) },
                { "date", typeof(DateTime) },
                { "datetime", typeof(DateTime) },
                { "time", typeof(DateTime) },
                { "dt", typeof(DateTime) },
                { "datetimeoffset", typeof(DateTimeOffset) },
                { "dto", typeof(DateTimeOffset) },
                { "timespan", typeof(TimeSpan) },
                { "ndate", typeof(LocalDate) },
                { "ntime", typeof(LocalTime) },
                { "ndatetime", typeof(LocalDateTime) },
                { "ndt", typeof(LocalDateTime) },
                { "ndatetimeoffset", typeof(OffsetDateTime) },
                { "ndto", typeof(OffsetDateTime) },
                { "zdatetime", typeof(ZonedDateTime) },
                { "zdt", typeof(ZonedDateTime) },
            };

        /// <summary>
        /// Returns the type object for a class name or type alias.  A type alias is not case sensitive.
        /// </summary>
        /// <param name="typeName">the fully qualified class name or type alias</param>
        /// <returns>the class, or null if the type name is invalid</returns>
        public static Type ToType(this string typeName)
        {
            Type result;
            if (!_wellKnownTypes.TryGetValue(typeName, out result))
                result = Type.GetType(typeName, false);
            return result;
        }

        /// <summary>
        /// Returns true if the type alias is not used to register a
        /// type handler for its associated class.
        /// </summary>
        /// <param name="alias">the type alias to check</param>
        /// <returns>true if the type alias is only an alias</returns>
        public static bool IsAliasOnly(this string alias)
        {
            Type result;
            if (!_wellKnownTypes.TryGetValue(alias, out result))
                return false;
            if (result == null)
                return false;
            return string.Equals(result.Name, alias, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets type <see cref="Type"/> for a given type name.
        /// </summary>
        /// <param name="typeName">The type name (<see cref="Type.AssemblyQualifiedName"/>).</param>
        /// <returns>The type for the given type name or null if it wasn't found.</returns>
        public static Type ToBeanType(this string typeName)
        {
            if (typeName == null)
                return null;
            switch (typeName.ToLowerInvariant())
            {
                case "map":
                case "dictionary":
                    return typeof(IDictionary<,>);
                case "list":
                case "collection":
                    return typeof(IList<>);
                case "set":
                    return typeof(ISet<>);
            }

            return Type.GetType(typeName, false);
        }

        /// <summary>
        /// Is the <paramref name="refType"/> assignable from an instance of <paramref name="testType"/>?
        /// </summary>
        /// <param name="refType">The reference type to test against</param>
        /// <param name="testType">The type to test for being an instance of <paramref name="refType"/></param>
        /// <returns>true, if <paramref name="testType"/> is an instance of <paramref name="refType"/></returns>
        public static bool IsAssignableFrom(this Type refType, Type testType)
        {
            return testType.IsInstanceOf(refType);
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
