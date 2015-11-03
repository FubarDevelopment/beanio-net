using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using BeanIO.Internal.Parser;

using JetBrains.Annotations;

using NodaTime;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// Type utility functions
    /// </summary>
    internal static class TypeUtil
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
                { "datetime", typeof(DateTime) },
                { "dt", typeof(DateTime) },
                { "datetimeoffset", typeof(DateTimeOffset) },
                { "dto", typeof(DateTimeOffset) },
                { "date", typeof(LocalDate) },
                { "time", typeof(LocalTime) },
                { "timespan", typeof(TimeSpan) },
                //// { "ndate", typeof(LocalDate) },
                //// { "ntime", typeof(LocalTime) },
                //// { "ndatetime", typeof(LocalDateTime) },
                //// { "ndt", typeof(LocalDateTime) },
                //// { "ndatetimeoffset", typeof(OffsetDateTime) },
                //// { "ndto", typeof(OffsetDateTime) },
                //// { "zdatetime", typeof(ZonedDateTime) },
                //// { "zdt", typeof(ZonedDateTime) },
            };

        private static readonly ConcurrentDictionary<string, bool> _isInstanceOfCache = new ConcurrentDictionary<string, bool>();

#if ALIAS_SUPPORT
        /// <summary>
        /// Get all the well known names for a given type.
        /// </summary>
        /// <param name="type">The type to return the well known names for</param>
        /// <returns>The well known names of the given <paramref name="type"/></returns>
        public static IEnumerable<string> GetWellKnownNamesFor(this Type type)
        {
            var fullName = type.GetAssemblyQualifiedName();
            return _wellKnownTypes.Where(x => x.Value.GetAssemblyQualifiedName() == fullName).Select(x => x.Key);
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
#endif

        public static bool CanSetTo(this object value, Type type)
        {
            return value != null || !type.GetTypeInfo().IsPrimitive || Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Returns the type object for a class name or type alias.  A type alias is not case sensitive.
        /// </summary>
        /// <param name="typeName">the fully qualified class name or type alias</param>
        /// <returns>the class, or null if the type name is invalid</returns>
        [CanBeNull]
        public static Type ToType(this string typeName)
        {
            Type result;
            if (!_wellKnownTypes.TryGetValue(typeName, out result))
                result = Type.GetType(typeName, false);
            return result;
        }

        /// <summary>
        /// Returns the collection <see cref="Type"/> object for a collection class name or type alias
        /// </summary>
        /// <param name="type">the fully qualified class name or type alias of the collection</param>
        /// <returns>the collection class, or <see cref="Array"/> for array,
        /// or <code>null</code> if the type name is invalid</returns>
        public static Type ToCollectionType(this string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "array":
                    return typeof(Array);
                case "collection":
                case "list":
                    return typeof(List<>);
                case "set":
                    return typeof(HashSet<>);
            }

            var clazz = Type.GetType(type);
            if (!typeof(IList).IsAssignableFrom(clazz))
                return null;
            return clazz;
        }

        public static Type CreateDefaultType(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (!type.IsConstructedGenericType && typeInfo.ContainsGenericParameters)
            {
                if (type.IsMap())
                    return typeof(Dictionary<string, object>);
                if (type.IsList())
                    return typeof(List<object>);
                if (type.IsInstanceOf(typeof(ISet<>)))
                    return typeof(HashSet<object>);
                return null;
            }
            return type;
        }

        public static Type ToAggregationType(this string type, IProperty property)
        {
            Type propertyType;
            if (property != null && property.PropertyType != null)
            {
                propertyType = property.PropertyType.CreateDefaultType();
            }
            else
            {
                propertyType = null;
            }

            switch (type.ToLowerInvariant())
            {
                case "array":
                    return typeof(Array);
                case "collection":
                case "list":
                    if (propertyType == null)
                        return typeof(IList);
                    return typeof(IList<>).MakeGenericType(propertyType);
                case "set":
                    if (propertyType == null)
                        return typeof(ISet<object>);
                    return typeof(ISet<>).MakeGenericType(propertyType);
                case "map":
                    return typeof(IDictionary<,>);
            }

            var clazz = Type.GetType(type, false);
            if (clazz == null)
            {
                var firstCommaPos = type.IndexOf(',');
                if (firstCommaPos != -1)
                    type = type.Substring(0, firstCommaPos);
                switch (type.Trim())
                {
                    case "System.Collections.Generic.HashSet`1":
                        clazz = typeof(HashSet<>);
                        break;
                    case "System.Collections.Generic.ISet`1":
                        clazz = typeof(ISet<>);
                        break;
                    default:
                        return null;
                }
            }
            if (clazz.IsSet())
            {
                if (propertyType != null && !clazz.IsConstructedGenericType && clazz.GetTypeInfo().ContainsGenericParameters)
                {
                    return clazz.MakeGenericType(propertyType);
                }
                return clazz;
            }
            if (clazz.IsList())
            {
                if (propertyType != null && !clazz.IsConstructedGenericType && clazz.GetTypeInfo().ContainsGenericParameters)
                {
                    return clazz.MakeGenericType(propertyType);
                }
                return clazz;
            }
            if (clazz.IsMap())
            {
                return clazz;
            }
            return null;
        }

        /// <summary>
        /// Gets type <see cref="Type"/> for a given type name.
        /// </summary>
        /// <param name="typeName">The type name (<see cref="Type.AssemblyQualifiedName"/>).</param>
        /// <param name="returnInterface">Return interface instead of concrete class</param>
        /// <returns>The type for the given type name or null if it wasn't found.</returns>
        public static Type ToBeanType(this string typeName, bool returnInterface = false)
        {
            if (typeName == null)
                return null;
            switch (typeName.ToLowerInvariant())
            {
                case "map":
                case "dictionary":
                    if (returnInterface)
                        return typeof(IDictionary<,>);
                    return typeof(Dictionary<,>);
                case "list":
                case "collection":
                    if (returnInterface)
                        return typeof(IList<>);
                    return typeof(List<>);
                case "set":
                    if (returnInterface)
                        return typeof(ISet<>);
                    return typeof(HashSet<>);
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
            var key = string.Concat(testType.GetAssemblyQualifiedName(), "|", refType.GetAssemblyQualifiedName());
            bool result = _isInstanceOfCache.GetOrAdd(key, _ => testType.IsInstanceOfInternal(refType));
            return result;
        }

        /// <summary>
        /// Get the assembly qualified name using the <see cref="Type.Namespace"/>, <see cref="Type.Name"/>, and <see cref="Assembly.FullName"/>
        /// </summary>
        /// <param name="t">The type to get the assembly qualified name for</param>
        /// <returns>The assembly qualified type name</returns>
        public static string GetAssemblyQualifiedName([CanBeNull] this Type t)
        {
            if (t == null)
                return null;
            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(t.FullName))
            {
                result.Append(t.FullName);
            }
            else
            {
                if (!string.IsNullOrEmpty(t.Namespace))
                    result.Append(t.Namespace).Append(".");
                result.Append(t.Name);
            }
            result.AppendFormat(", {0}", t.GetTypeInfo().Assembly.FullName);
            return result.ToString();
        }

        /// <summary>
        /// Returns <code>true</code>, when the type is a numeric type
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to test</param>
        /// <returns><code>true</code>, when the type is a numeric type</returns>
        public static bool IsNumber(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive || Nullable.GetUnderlyingType(type) != null;
        }

        public static IList AsList(this object value)
        {
            if (value == null)
                return null;
            var list = (value as IList) ?? new SetProxyList((IEnumerable)value);
            return list;
        }

        public static Type GetElementType(this ICollection list)
        {
            var listType = list.GetType();
            if (listType.IsInstanceOf(typeof(IList<>))
                || listType.IsInstanceOf(typeof(ICollection<>))
                || listType.IsInstanceOf(typeof(IReadOnlyList<>))
                || listType.IsInstanceOf(typeof(IReadOnlyCollection<>)))
                return listType.GenericTypeArguments[0];
            if (listType.IsArray)
                return listType.GetElementType();
            if (listType.IsInstanceOf(typeof(ISet<>)))
                return listType.GenericTypeArguments[0];
            return typeof(object);
        }

        public static bool IsArray(this Type type)
        {
            return type.IsArray || type == typeof(Array);
        }

        public static bool IsCollection(this Type type)
        {
            return typeof(ICollection).IsAssignableFrom(type)
                   || typeof(ICollection<>).IsAssignableFrom(type)
                   || typeof(IReadOnlyCollection<>).IsAssignableFrom(type);
        }

        public static bool IsList(this Type type)
        {
            return typeof(IList).IsAssignableFrom(type)
                   || typeof(IList<>).IsAssignableFrom(type)
                   || typeof(IReadOnlyList<>).IsAssignableFrom(type)
                   || (type.IsCollection() && !type.IsMap());
        }

        public static bool IsMap(this Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type)
                   || typeof(IDictionary<,>).IsAssignableFrom(type)
                   || typeof(IReadOnlyDictionary<,>).IsAssignableFrom(type);
        }

        public static bool IsSet(this Type type)
        {
            return typeof(ISet<>).IsAssignableFrom(type);
        }

        /// <summary>
        /// Instantiate a generic type using the type arguments from <paramref name="with"/>
        /// </summary>
        /// <param name="type">The new types instance</param>
        /// <param name="with">The type to get the generic arguments from</param>
        /// <returns>The constructed generic type from <paramref name="type"/></returns>
        public static Type Instantiate(this Type type, Type with)
        {
            var typeInfo = type.GetTypeInfo();
            if (!type.IsConstructedGenericType && typeInfo.IsGenericTypeDefinition && (with == null || with == typeof(object)))
            {
                if (type.IsMap())
                    return type.MakeGenericType(typeof(string), typeof(object));
                return type.MakeGenericType(typeof(object));
            }

            if (with == null || !with.IsConstructedGenericType)
            {
                return type;
            }

            var withInfo = with.GetTypeInfo();
            if (typeInfo.GenericTypeParameters.Length != withInfo.GenericTypeArguments.Length)
                throw new InvalidOperationException("The number of type arguments must match");
            return type.MakeGenericType(with.GenericTypeArguments);
        }

        /// <summary>
        /// Is the <paramref name="testType"/> an instance of <paramref name="refType"/>?
        /// </summary>
        /// <param name="testType">The type to test for being an instance of <paramref name="refType"/></param>
        /// <param name="refType">The reference type to test against</param>
        /// <returns>true, if <paramref name="testType"/> is an instance of <paramref name="refType"/></returns>
        private static bool IsInstanceOfInternal(this Type testType, Type refType)
        {
            if (testType.IsConstructedGenericType && !refType.IsConstructedGenericType)
            {
                testType = testType.GetGenericTypeDefinition();
            }
            else if (!testType.IsConstructedGenericType && refType.IsConstructedGenericType)
            {
                var nonNullableType = Nullable.GetUnderlyingType(refType);
                if (nonNullableType == null)
                {
                    refType = refType.GetGenericTypeDefinition();
                }
                else
                {
                    refType = nonNullableType;
                }
            }

            var refTypeInfo = refType.GetTypeInfo();
            var testTypeInfo = testType.GetTypeInfo();
            if (refTypeInfo.IsAssignableFrom(testTypeInfo))
                return true;

            var comparer = new TypeComparer();

            if (comparer.Compare(refType, testType) == 0)
                return true;

            foreach (var implementedInterface in testTypeInfo.ImplementedInterfaces)
            {
                if (comparer.Compare(refType, implementedInterface) == 0)
                    return true;

                if (!testTypeInfo.IsInterface)
                {
                    var ifMap = testTypeInfo.GetRuntimeInterfaceMap(implementedInterface);
                    if (ifMap.InterfaceType != null)
                    {
                        if (comparer.Compare(refType, ifMap.InterfaceType) == 0)
                            return true;
                    }
                }
            }
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
                var name1 = hasMissingFullName ? x.GetAssemblyQualifiedName() : x.AssemblyQualifiedName;
                var name2 = hasMissingFullName ? y.GetAssemblyQualifiedName() : y.AssemblyQualifiedName;
                return StringComparer.Ordinal.Compare(name1, name2);
            }
        }
    }
}
