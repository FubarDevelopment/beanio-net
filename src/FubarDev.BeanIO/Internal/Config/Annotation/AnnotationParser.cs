// <copyright file="AnnotationParser.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml;

using BeanIO.Annotation;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Config.Annotation
{
    /// <summary>
    /// Factory class for building component configurations from annotated classes.
    /// </summary>
    internal static class AnnotationParser
    {
        private static readonly OrdinalComparer _ordinalComparer = new OrdinalComparer();

        /// <summary>
        /// Creates a <see cref="GroupConfig"/> from the given type, if the type is annotated
        /// using <see cref="GroupAttribute"/>.
        /// </summary>
        /// <param name="typeName">The bean type name.</param>
        /// <returns>the <see cref="GroupConfig"/> or null if the class was not annotated.</returns>
        public static GroupConfig? CreateGroupConfig(string? typeName)
        {
            var clazz = typeName.ToBeanType();
            if (clazz == null)
                return null;
            return CreateGroupConfig(clazz);
        }

        /// <summary>
        /// Creates a <see cref="GroupConfig"/> from the given type, if the type is annotated
        /// using <see cref="GroupAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The group type.</typeparam>
        /// <returns>the <see cref="GroupConfig"/> or null if the class was not annotated.</returns>
        public static GroupConfig? CreateGroupConfig<T>()
        {
            return CreateGroupConfig(typeof(T));
        }

        /// <summary>
        /// Creates a <see cref="GroupConfig"/> from the given type, if the type is annotated
        /// using <see cref="GroupAttribute"/>.
        /// </summary>
        /// <param name="type">the group type.</param>
        /// <returns>the <see cref="GroupConfig"/> or null if the class was not annotated.</returns>
        public static GroupConfig? CreateGroupConfig(Type type)
        {
            var typeInfo = type;
            var group = typeInfo.GetCustomAttribute<GroupAttribute>();
            if (group == null)
                return null;

            var name = group.Name.ToValue()
                       ?? Introspector.Decapitalize(type.Name);

            var info = new TypeInfo()
                {
                    Type = type,
                    Name = name,
                };

            return CreateGroup(info, group);
        }

        /// <summary>
        /// Creates a <see cref="RecordConfig"/> from the given type, if the type is annotated
        /// using <see cref="RecordAttribute"/>.
        /// </summary>
        /// <param name="typeName">The bean type name.</param>
        /// <returns>the <see cref="RecordConfig"/> or null if the class was not annotated.</returns>
        public static RecordConfig? CreateRecordConfig(string? typeName)
        {
            var clazz = typeName.ToBeanType();
            if (clazz == null)
                return null;
            return CreateRecordConfig(clazz);
        }

        /// <summary>
        /// Creates a <see cref="RecordConfig"/> from the given type, if the type is annotated
        /// using <see cref="RecordAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The record type.</typeparam>
        /// <returns>the <see cref="RecordConfig"/> or null if the class was not annotated.</returns>
        public static RecordConfig? CreateRecordConfig<T>()
        {
            return CreateRecordConfig(typeof(T));
        }

        /// <summary>
        /// Creates a <see cref="RecordConfig"/> from the given type, if the type is annotated
        /// using <see cref="RecordAttribute"/>.
        /// </summary>
        /// <param name="type">the record type.</param>
        /// <returns>the <see cref="RecordConfig"/> or null if the class was not annotated.</returns>
        public static RecordConfig? CreateRecordConfig(Type type)
        {
            var typeInfo = type;
            var record = typeInfo.GetCustomAttribute<RecordAttribute>();
            if (record == null)
                return null;

            var name = record.Name.ToValue()
                       ?? Introspector.Decapitalize(type.Name);

            var info = new TypeInfo()
            {
                Type = type,
                Name = name,
            };

            return CreateRecord(info, record);
        }

        private static GroupConfig CreateGroup(TypeInfo info, GroupAttribute group)
        {
            UpdateTypeInfo(info, group.Type, group.CollectionType);

            var minOccurs = group.MinOccurs.ToValue();
            var maxOccurs = group.MaxOccurs.ToUnboundedValue();
            if (maxOccurs == null && info.CollectionName == null && info.IsBound)
                maxOccurs = 1;

            var gc = new GroupConfig()
            {
                Name = info.Name,
                Type = info.PropertyName,
                Collection = info.CollectionName,

                MinOccurs = minOccurs,
                MaxOccurs = maxOccurs,

                ValidateOnMarshal = ToValue(group.ValidationMode),

                XmlType = group.XmlType.ToValue(),
                XmlName = group.XmlName.ToValue(),
                XmlNamespace = group.XmlNamespace.ToValue(),
                XmlPrefix = group.XmlPrefix.ToValue(),
            };

            if (info.PropertyType != null)
            {
                AddAllChildren(gc, info.PropertyType);
            }

            return gc;
        }

        private static RecordConfig CreateRecord(TypeInfo info, RecordAttribute record)
        {
            UpdateTypeInfo(info, record.Type, record.CollectionType);

            var minOccurs = record.MinOccurs.ToValue();
            var maxOccurs = record.MaxOccurs.ToUnboundedValue();
            if (maxOccurs == null && info.CollectionName == null && info.IsBound)
                maxOccurs = 1;

            var target = record.Value.ToValue();

            var rc = new RecordConfig()
            {
                Name = info.Name,
                Target = target,
                Type = target == null ? info.PropertyName : null,
                Collection = info.CollectionName,
                Order = record.Order.ToValue(),

                MinOccurs = minOccurs,
                MaxOccurs = maxOccurs,

                MinLength = record.MinLength.ToValue(),
                MaxLength = record.MaxLength.ToUnboundedValue(),
                MinMatchLength = record.MinRecordIdentificationLength.ToValue(),
                MaxMatchLength = record.MaxRecordIdentificationLength.ToUnboundedValue(),

                ValidateOnMarshal = ToValue(record.ValidationMode),

                XmlType = record.XmlType.ToValue(),
                XmlName = record.XmlName.ToValue(),
                XmlNamespace = record.XmlNamespace.ToValue(),
                XmlPrefix = record.XmlPrefix.ToValue(),
            };

            var fields = info.PropertyType?.GetCustomAttributes<FieldAttribute>();
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    rc.Add(CreateField(null, field));
                }
            }

            if (info.PropertyType != null)
            {
                HandleConstructor(rc, info.PropertyType);
                AddAllChildren(rc, info.PropertyType);
            }

            rc.Sort(_ordinalComparer);

            return rc;
        }

        private static FieldConfig CreateField(TypeInfo? info, FieldAttribute fa)
        {
            FieldConfig fc;
            if (info != null)
            {
                UpdateTypeInfo(info, fa.Type, fa.CollectionType);

                var setter = fa.Setter.ToValue();
                if (info.ArgumentIndex != null)
                {
                    if (setter != null)
                        throw new BeanIOConfigurationException("setter not allowed");
                    setter = $"#{info.ArgumentIndex}";
                }
                else
                {
                    setter = setter ?? info.Setter;
                }

                fc = new FieldConfig()
                    {
                        Name = info.Name,
                        Label = fa.Name.ToValue(),
                        Type = info.PropertyName,
                        Collection = info.CollectionName,
                        IsBound = true,
                        Getter = fa.Getter.ToValue() ?? info.Getter,
                        Setter = setter,
                    };
            }
            else
            {
                fc = new FieldConfig()
                    {
                        Name = fa.Name.ToValue() ?? throw new BeanIOConfigurationException($"Name is required for field {fa}"),
                        Label = fa.Name.ToValue(),
                        IsBound = false,
                    };
            }

            if (string.IsNullOrEmpty(fc.Name))
                throw new BeanIOConfigurationException("name is required");

            fc.ValidateOnMarshal = ToValue(fa.ValidationMode);
            fc.Literal = fa.Literal.ToValue();
            fc.Position = fa.At.ToValue();
            fc.Until = fa.Until.ToValue();
            fc.Ordinal = fa.Ordinal.ToValue();
            fc.RegEx = fa.RegEx.ToValue();
            fc.Format = fa.Format.ToValue();
            fc.IsRequired = fa.IsRequired;
            fc.DefaultValue = fa.DefaultValue.ToValue();
            fc.IsIdentifier = fa.IsRecordIdentifier;
            fc.IsTrim = fa.Trim;
            fc.IsLazy = fa.IsLazy;
            fc.MinLength = fa.MinLength.ToValue();
            fc.MaxLength = fa.MaxLength.ToUnboundedValue();
            fc.MinOccurs = fa.MinOccurs.ToValue();
            fc.MaxOccurs = fa.MaxOccurs.ToUnboundedValue();
            fc.OccursRef = fa.OccursRef.ToValue();

            fc.Length = fa.Length.ToValue();
            fc.Padding = (fa.Padding >= char.MinValue && fa.Padding <= char.MaxValue) ? char.ConvertFromUtf32(fa.Padding)[0] : (char?)null;
            fc.Justify = fa.Align;
            fc.KeepPadding = fa.KeepPadding;
            if (fa.ParseDefault != ParseDefaultBehavior.Default)
                fc.ParseDefault = fa.ParseDefault == ParseDefaultBehavior.Parse;
            fc.IsLenientPadding = fa.LenientPadding;

            fc.TypeHandler = fa.HandlerName.ToValue();
            var handler = fa.HandlerType.ToValue();
            if (handler != null && string.IsNullOrEmpty(fc.TypeHandler))
                fc.TypeHandler = fa.HandlerType.GetAssemblyQualifiedName();

            fc.XmlType = fa.XmlType.ToValue();
            fc.XmlName = fa.XmlName.ToValue();
            fc.XmlNamespace = fa.XmlNamespace.ToValue();
            fc.XmlPrefix = fa.XmlPrefix.ToValue();
            fc.IsNillable = fa.IsNullable;

            return fc;
        }

        private static SegmentConfig CreateSegment(TypeInfo info, SegmentAttribute sa, IReadOnlyCollection<FieldAttribute>? fields)
        {
            UpdateTypeInfo(info, sa.Type, sa.CollectionType);

            if (info.PropertyType == typeof(string))
                throw new BeanIOConfigurationException("type is undefined");

            var target = sa.Value.ToValue();

            var sc = new SegmentConfig()
                {
                    Name = info.Name,
                    Label = sa.Name.ToValue(),
                    Target = target,
                    Type = target == null ? info.PropertyName : null,
                    Collection = info.CollectionName,
                    Getter = sa.Getter.ToValue() ?? info.Getter,
                    Setter = sa.Setter.ToValue() ?? info.Setter,
                    Position = sa.At.ToValue(),
                    Until = sa.Until.ToValue(),
                    Ordinal = sa.Ordinal.ToValue(),
                    MinOccurs = sa.MinOccurs.ToValue(),
                    MaxOccurs = sa.MaxOccurs.ToUnboundedValue(),
                    OccursRef = sa.OccursRef.ToValue(),
                    IsLazy = sa.IsLazy,
                    ValidateOnMarshal = ToValue(sa.ValidationMode),
                    XmlType = sa.XmlType.ToValue(),
                    XmlName = sa.XmlName.ToValue(),
                    XmlNamespace = sa.XmlNamespace.ToValue(),
                    XmlPrefix = sa.XmlPrefix.ToValue(),
                };

            sc.SetKey(sa.Key.ToValue());

            if (string.IsNullOrEmpty(sc.Name))
                throw new BeanIOConfigurationException("name is undefined");

            if (fields != null)
            {
                foreach (var field in fields)
                {
                    sc.Add(CreateField(null, field));
                }
            }

            fields = info.PropertyType?.GetCustomAttributes<FieldAttribute>().ToList()
                     ?? (IReadOnlyCollection<FieldAttribute>)Array.Empty<FieldAttribute>();
            foreach (var field in fields)
            {
                sc.Add(CreateField(null, field));
            }

            if (info.PropertyType != null)
            {
                HandleConstructor(sc, info.PropertyType);
                AddAllChildren(sc, info.PropertyType);
            }

            return sc;
        }

        private static void HandleConstructor(ComponentConfig config, Type clazz)
        {
            try
            {
                var typeInfo = clazz;
                foreach (var constructor in typeInfo.GetConstructors())
                {
                    var parameters = constructor.GetParameters();
                    for (var i = 0; i != parameters.Length; ++i)
                    {
                        var parameter = parameters[i];
                        var fa = parameter.GetCustomAttribute<FieldAttribute>();
                        if (fa == null)
                            continue;

                        var info = new TypeInfo
                        {
                            ArgumentIndex = i + 1,
                            Name = fa.Name.ToValue() ?? parameter.Name
                                ?? throw new InvalidOperationException("Parameter name is required"),
                            Type = parameter.ParameterType,
                        };

                        config.Add(CreateField(info, fa));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BeanIOConfigurationException(
                    $"Invalid FieldAttribute annotation on a constructor parameter in class '{clazz.GetAssemblyQualifiedName()}': {ex.Message}",
                    ex);
            }
        }

        private static void AddAllChildren(ComponentConfig config, Type type)
        {
            var typeInfo = type;

            var baseClass = typeInfo.BaseType;
            if (baseClass != null && baseClass != typeof(object))
            {
                AddAllChildren(config, baseClass);
            }

            foreach (var interfaceType in typeInfo.GetInterfaces())
            {
                AddAllChildren(config, interfaceType);
            }

            AddChildren(config, type);
        }

        private static void AddChildren(ComponentConfig config, Type parent)
        {
            if (config.ComponentType == ComponentType.Group)
                AddGroupChildren(config, parent);
            else
                AddRecordChildren(config, parent);
        }

        private static void AddGroupChildren(ComponentConfig config, Type parent)
        {
            var typeInfo = parent;

            var fieldsAndProperties = typeInfo
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.DeclaringType == typeInfo)
                .Where(x => !x.Name.EndsWith(">k__BackingField"))
                .Cast<MemberInfo>()
                .Union(typeInfo
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.DeclaringType == typeInfo));

            // Fields and properties
            foreach (var fieldOrProperty in fieldsAndProperties)
            {
                var ga = fieldOrProperty.GetCustomAttribute<GroupAttribute>();
                var ra = fieldOrProperty.GetCustomAttribute<RecordAttribute>();
                if (ra == null && ga == null)
                    continue;
                if (ra != null && ga != null)
                {
                    throw new BeanIOConfigurationException(
                        $"Field/Property '{fieldOrProperty.Name}' on class '{parent.Name}' cannot be annotated with both RecordAttribute and GroupAttribute.");
                }

                var fieldInfo = fieldOrProperty as FieldInfo;
                var propInfo = fieldOrProperty as PropertyInfo;
                Debug.Assert(fieldInfo != null || propInfo != null, "fieldInfo != null || propInfo != null");

                var info = new TypeInfo
                {
                    IsBound = true,
                    Name = fieldOrProperty.Name,
                    Type =
                        fieldInfo != null
                            ? fieldInfo.FieldType
                            : propInfo!.PropertyType,
                };

                PropertyConfig child;
                try
                {
                    if (ra != null)
                        child = CreateRecord(info, ra);
                    else
                        child = CreateGroup(info, ga!);
                }
                catch (ArgumentException ex)
                {
                    throw new BeanIOConfigurationException(
                        $"Invalid annotation for field/property '{fieldOrProperty.Name}' on class '{parent.Name}': {ex.Message}",
                        ex);
                }

                config.Add(child);
            }

            // Methods (setter/getter)
            foreach (var method in typeInfo.GetMethods())
            {
                var ga = method.GetCustomAttribute<GroupAttribute>();
                var ra = method.GetCustomAttribute<RecordAttribute>();
                if (ra == null && ga == null)
                    continue;
                if (ra != null && ga != null)
                {
                    throw new BeanIOConfigurationException(
                        $"Method '{method.Name}' on class '{parent.Name}' cannot be annotated with both RecordAttribute and GroupAttribute.");
                }

                Type clazz;
                var name = method.Name;
                string? getter, setter;

                var parameters = method.GetParameters();
                if (method.ReturnType != typeof(void) && parameters.Length == 0)
                {
                    // Getter
                    getter = name;
                    setter = null;
                    clazz = method.ReturnType;
                    if (name.StartsWith("Get") || name.StartsWith("get"))
                        name = name.Substring(3);
                    else if (name.StartsWith("Is") || name.StartsWith("is"))
                        name = name.Substring(2);
                }
                else if (method.ReturnType == typeof(void) && parameters.Length == 1)
                {
                    // Setter
                    getter = null;
                    setter = name;
                    clazz = parameters[0].ParameterType;
                    if (name.StartsWith("Set") || name.StartsWith("set"))
                        name = name.Substring(3);
                }
                else
                {
                    throw new BeanIOConfigurationException($"Method '{method.Name}' on class '{parent.Name}' is not a valid getter or setter");
                }

                name = Introspector.Decapitalize(name);
                var info = new TypeInfo()
                    {
                        IsBound = true,
                        Name = name,
                        Type = clazz,
                        Getter = getter,
                        Setter = setter,
                    };

                PropertyConfig child;
                try
                {
                    if (ra != null)
                        child = CreateRecord(info, ra);
                    else
                        child = CreateGroup(info, ga!);
                }
                catch (ArgumentException ex)
                {
                    throw new BeanIOConfigurationException($"Invalid annotation for method '{method.Name}' on class '{parent.Name}': {ex.Message}", ex);
                }

                config.Add(child);
            }
        }

        private static void AddRecordChildren(ComponentConfig config, Type parent)
        {
            var typeInfo = parent;

            var fieldsAndProperties = typeInfo
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.DeclaringType == typeInfo)
                .Where(x => !x.Name.EndsWith(">k__BackingField"))
                .Cast<MemberInfo>()
                .Union(typeInfo
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.DeclaringType == typeInfo));

            // Fields and properties
            foreach (var fieldOrProperty in fieldsAndProperties)
            {
                var fas = fieldOrProperty.GetCustomAttributes<FieldAttribute>().ToList();
                var sa = fieldOrProperty.GetCustomAttribute<SegmentAttribute>();
                if (fas.Count == 0 && sa == null)
                    continue;
                if (fas.Count > 1 && sa == null)
                {
                    throw new BeanIOConfigurationException(
                        $"Field/Property '{fieldOrProperty.Name}' on class '{parent.Name}' cannot be annotated with multiple FieldAttribute without SegmentAttribute.");
                }

                var fieldInfo = fieldOrProperty as FieldInfo;
                var propInfo = fieldOrProperty as PropertyInfo;
                Debug.Assert(fieldInfo != null || propInfo != null, "fieldInfo != null || propInfo != null");

                var info = new TypeInfo
                {
                    Name = fieldOrProperty.Name,
                    Type = fieldInfo == null ? propInfo!.PropertyType : fieldInfo.FieldType,
                };

                PropertyConfig child;
                try
                {
                    if (sa == null)
                        child = CreateField(info, fas.Single());
                    else
                        child = CreateSegment(info, sa, fas);
                }
                catch (ArgumentException ex)
                {
                    throw new BeanIOConfigurationException(
                        $"Invalid annotation for field/property '{fieldOrProperty.Name}' on class '{parent.Name}': {ex.Message}",
                        ex);
                }

                config.Add(child);
            }

            // Methods (setter/getter)
            foreach (var method in typeInfo.GetMethods())
            {
                var fas = method.GetCustomAttributes<FieldAttribute>().ToList();
                var sa = method.GetCustomAttribute<SegmentAttribute>();
                if (fas.Count == 0 && sa == null)
                    continue;
                if (fas.Count > 1 && sa == null)
                {
                    throw new BeanIOConfigurationException(
                        $"Method '{method.Name}' on class '{parent.Name}' cannot be annotated with multiple FieldAttribute without SegmentAttribute.");
                }

                Type clazz;
                var name = method.Name;
                string? getter, setter;

                var parameters = method.GetParameters();
                if (method.ReturnType != typeof(void) && parameters.Length == 0)
                {
                    // Getter
                    getter = name;
                    setter = null;
                    clazz = method.ReturnType;
                    if (name.StartsWith("Get") || name.StartsWith("get"))
                        name = name.Substring(3);
                    else if (name.StartsWith("Is") || name.StartsWith("is"))
                        name = name.Substring(2);
                }
                else if (method.ReturnType == typeof(void) && parameters.Length == 1)
                {
                    // Setter
                    getter = null;
                    setter = name;
                    clazz = parameters[0].ParameterType;
                    if (name.StartsWith("Set") || name.StartsWith("set"))
                        name = name.Substring(3);
                }
                else
                {
                    throw new BeanIOConfigurationException($"Method '{method.Name}' on class '{parent.Name}' is not a valid getter or setter");
                }

                name = Introspector.Decapitalize(name);
                var info = new TypeInfo()
                {
                    IsBound = true,
                    Name = name,
                    Type = clazz,
                    Getter = getter,
                    Setter = setter,
                };

                PropertyConfig child;
                try
                {
                    if (sa == null)
                        child = CreateField(info, fas.Single());
                    else
                        child = CreateSegment(info, sa, fas);
                }
                catch (ArgumentException ex)
                {
                    throw new BeanIOConfigurationException($"Invalid annotation for method '{method.Name}' on class '{parent.Name}': {ex.Message}", ex);
                }

                config.Add(child);
            }
        }

        private static void UpdateTypeInfo(TypeInfo info, Type? annotatedType, Type? annotatedCollection)
        {
            annotatedType = annotatedType.ToValue();

            string? collectionName;
            var propertyType = info.Type;

            if (propertyType.IsArray || propertyType == typeof(Array))
            {
                propertyType = annotatedType ?? propertyType.GetElementType();
                collectionName = "array";
            }
            else if (propertyType.IsMap())
            {
                var collectionType = annotatedCollection.ToValue();
                if (collectionType == null)
                {
                    collectionType = propertyType;
                    propertyType = null;
                }

                if (annotatedType != null)
                {
                    propertyType = annotatedType;
                }
                else
                {
                    if (info.Type.IsConstructedGenericType)
                    {
                        propertyType = info.Type.GenericTypeArguments[1];
                    }
                    else if (propertyType == null)
                    {
                        propertyType = typeof(string);
                    }
                }

                collectionName = collectionType.GetAssemblyQualifiedName();
            }
            else if (propertyType.IsList())
            {
                var collectionType = annotatedCollection.ToValue();
                if (collectionType == null)
                {
                    collectionType = propertyType;
                    propertyType = null;
                }

                if (annotatedType != null)
                {
                    propertyType = annotatedType;
                }
                else
                {
                    if (info.Type.IsConstructedGenericType)
                    {
                        propertyType = info.Type.GenericTypeArguments[0];
                    }
                    else if (propertyType == null)
                    {
                        propertyType = typeof(string);
                    }
                }

                collectionName = collectionType.GetAssemblyQualifiedName();
            }
            else
            {
                if (annotatedType != null)
                    propertyType = annotatedType;
                collectionName = null;
            }

            info.PropertyType = propertyType;
            info.PropertyName = propertyType.GetAssemblyQualifiedName();
            info.CollectionName = collectionName;
        }

        private static Type? ToValue(this Type? type)
        {
            return type == null || type == typeof(void) ? null : type;
        }

        private static int? ToValue(this int? n)
        {
            return !n.HasValue || n.Value == int.MinValue ? null : n;
        }

        private static int? ToValue(this int n)
        {
            return n == int.MinValue ? (int?)null : n;
        }

        private static string? ToValue(this string? s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }

        private static XmlNodeType? ToValue(this XmlNodeType n)
        {
            return n == XmlNodeType.None ? (XmlNodeType?)null : n;
        }

        [return: NotNullIfNotNull(nameof(n))]
        private static int? ToUnboundedValue(this int? n)
        {
            var val = n.ToValue();
            if (val == null)
                return null;
            if (val < 0)
            {
                // maximum value (i.e. unbounded)
                return int.MaxValue;
            }

            return val.Value;
        }

        private static int? ToUnboundedValue(this int n)
        {
            if (n == int.MinValue)
                return null;
            return ToUnboundedValue((int?)n).Value;
        }

        private static bool? ToValue(ValidationMode mode)
        {
            if (mode == ValidationMode.NoValidateOnMarshal)
                return false;
            if (mode == ValidationMode.ValidateOnMarshal)
                return true;
            return null;
        }

        private class TypeInfo
        {
            public bool IsBound { get; set; }

            public int? ArgumentIndex { get; set; }

            public required string Name { get; set; }

            public required Type Type { get; set; }

            /// <summary>
            /// Gets or sets the class name of propertyType(?).
            /// </summary>
            public string? PropertyName { get; set; }

            public string? CollectionName { get; set; }

            public Type? PropertyType { get; set; }

            public string? Getter { get; set; }

            public string? Setter { get; set; }
        }

        private class OrdinalComparer : IComparer<ComponentConfig>, IEqualityComparer<ComponentConfig>
        {
            public int Compare(ComponentConfig? x, ComponentConfig? y)
            {
                var o1 = x?.Ordinal;
                var o2 = y?.Ordinal;
                if (o1 == null)
                    return o2 == null ? 0 : 1;
                if (o2 == null)
                    return -1;
                return o1.Value.CompareTo(o2.Value);
            }

            public bool Equals(ComponentConfig? x, ComponentConfig? y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                return Compare(x, y) == 0;
            }

            public int GetHashCode(ComponentConfig? obj)
            {
                return obj?.Ordinal.GetValueOrDefault().GetHashCode() ?? 0;
            }
        }
    }
}
