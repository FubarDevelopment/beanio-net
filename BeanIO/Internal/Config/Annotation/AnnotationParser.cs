using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BeanIO.Annotation;
using BeanIO.Internal.Util;

using JetBrains.Annotations;

namespace BeanIO.Internal.Config.Annotation
{
    public static class AnnotationParser
    {
        private static readonly OrdinalComparer _ordinalComparer = new OrdinalComparer();

        public static GroupConfig CreateGroupConfig(string typeName)
        {
            var clazz = typeName.ToBeanType();
            if (clazz == null)
                return null;
            return CreateGroupConfig(clazz);
        }

        public static GroupConfig CreateGroupConfig<T>()
        {
            return CreateGroupConfig(typeof(T));
        }

        public static GroupConfig CreateGroupConfig(Type type)
        {
            var typeInfo = type.GetTypeInfo();
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

        private static GroupConfig CreateGroup(TypeInfo info, GroupAttribute group)
        {
            UpdateTypeInfo(info, group.Type, group.CollectionType);

            var minOccurs = group.MinOccurs.ToValue().GetValueOrDefault();
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

                    XmlType = group.XmlType,
                    XmlName = group.XmlName,
                    XmlNamespace = group.XmlNamespace,
                    XmlPrefix = group.XmlPrefix,
                };

            AddAllChildren(gc, info.PropertyType);

            return gc;
        }

        private static RecordConfig CreateRecord(TypeInfo info, RecordAttribute record)
        {
            UpdateTypeInfo(info, record.Type, record.CollectionType);

            var minOccurs = record.MinOccurs.ToValue().GetValueOrDefault();
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

                XmlType = record.XmlType,
                XmlName = record.XmlName,
                XmlNamespace = record.XmlNamespace,
                XmlPrefix = record.XmlPrefix,
            };

            var fields = info.PropertyType.GetTypeInfo().GetCustomAttributes<FieldAttribute>();
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    rc.Add(CreateField(null, field));
                }
            }

            HandleConstructor(rc, info.PropertyType);
            AddAllChildren(rc, info.PropertyType);
            rc.Sort(_ordinalComparer);

            return rc;
        }

        private static FieldConfig CreateField(TypeInfo info, FieldAttribute fa)
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
                    setter = string.Format("#{0}", info.ArgumentIndex);
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
                        Name = fa.Name.ToValue(),
                        Label = fa.Name.ToValue(),
                        IsBound = false,
                    };
            }

            if (string.IsNullOrEmpty(fc.Name))
                throw new BeanIOConfigurationException("name is required");

            fc.Literal = fa.Literal.ToValue();
            fc.Position = fa.At.ToValue();
            fc.Until = fa.Until.ToValue();
            fc.Ordinal = fa.Ordinal.ToValue();
            fc.RegEx = fa.RegEx.ToValue();
            fc.Format = fa.Format.ToValue();
            fc.IsRequired = fa.IsRequired;
            fc.Default = fa.DefaultValue.ToValue();
            fc.IsIdentifier = fa.IsRecordIdentifier;
            fc.IsTrim = fa.Trim;
            fc.IsLazy = fa.IsLazy;
            fc.MinLength = fa.MinLength.ToValue();
            fc.MaxLength = fa.MaxLength.ToUnboundedValue();
            fc.MinOccurs = fa.MinOccurs.ToValue().GetValueOrDefault();
            fc.MaxOccurs = fa.MaxOccurs.ToUnboundedValue();
            fc.OccursRef = fa.OccursRef.ToValue();

            fc.Length = fa.Length.ToValue();
            fc.Padding = (fa.Padding >= char.MinValue && fa.Padding <= char.MaxValue) ? (char?)null : char.ConvertFromUtf32(fa.Padding)[0];
            fc.Justify = fa.Align;
            fc.KeepPadding = fa.KeepPadding;
            fc.IsLenientPadding = fa.LenientPadding;

            fc.TypeHandler = fa.HandlerName.ToValue();
            var handler = fa.HandlerType.ToValue();
            if (handler != null && string.IsNullOrEmpty(fc.TypeHandler))
                fc.TypeHandler = fa.HandlerType.GetFullName();

            fc.XmlType = fa.XmlType;
            fc.XmlName = fa.XmlName;
            fc.XmlNamespace = fa.XmlNamespace;
            fc.XmlPrefix = fa.XmlPrefix;
            fc.IsNillable = fa.IsNullable;

            return fc;
        }

        private static SegmentConfig CreateSegment(TypeInfo info, SegmentAttribute segment, IReadOnlyCollection<FieldAttribute> fields)
        {
        }

        private static void HandleConstructor(ComponentConfig config, Type clazz)
        {
            try
            {
                var typeInfo = clazz.GetTypeInfo();
                foreach (var constructor in typeInfo.DeclaredConstructors)
                {
                    var parameters = constructor.GetParameters();
                    var index = 0;
                    foreach (var parameter in parameters)
                    {
                        var fa = parameter.GetCustomAttribute<FieldAttribute>();
                        if (fa == null)
                            continue;

                        var info = new TypeInfo
                            {
                                ArgumentIndex = ++index,
                                Name = fa.Name.ToValue(),
                                Type = parameter.ParameterType,
                            };

                        config.Add(CreateField(info, fa));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BeanIOConfigurationException(string.Format("Invalid FieldAttribute annotation on a constructor parameter in class '{0}': {1}", clazz.GetFullName(), ex.Message), ex);
            }
        }

        private static void AddAllChildren(ComponentConfig config, Type type)
        {
            var typeInfo = type.GetTypeInfo();

            var baseClass = typeInfo.BaseType;
            if (baseClass != null && baseClass != typeof(object))
            {
                AddAllChildren(config, baseClass);
            }

            foreach (var interfaceType in typeInfo.ImplementedInterfaces)
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
            var typeInfo = parent.GetTypeInfo();

            var fieldsAndProperties = typeInfo.DeclaredFields.Cast<MemberInfo>().Union(typeInfo.DeclaredProperties);

            // Fields and properties
            foreach (var fieldOrProperty in fieldsAndProperties)
            {
                var ga = fieldOrProperty.GetCustomAttribute<GroupAttribute>();
                var ra = fieldOrProperty.GetCustomAttribute<RecordAttribute>();
                if (ra == null && ga == null)
                    continue;
                if (ra != null && ga != null)
                    throw new BeanIOConfigurationException(
                        string.Format(
                            "Field/Property '{0}' on class '{1}' cannot be annotated with both RecordAttribute and GroupAttribute.",
                            fieldOrProperty.Name,
                            parent.Name));

                var fieldInfo = fieldOrProperty as FieldInfo;
                var propInfo = fieldOrProperty as PropertyInfo;

                var info = new TypeInfo
                    {
                        IsBound = true,
                        Name = fieldOrProperty.Name,
                        Type = fieldInfo == null ? propInfo.PropertyType : fieldInfo.FieldType,
                    };

                PropertyConfig child;
                try
                {
                    if (ra != null)
                        child = CreateRecord(info, ra);
                    else
                        child = CreateGroup(info, ga);
                }
                catch (ArgumentException ex)
                {
                    throw new BeanIOConfigurationException(
                        string.Format(
                            "Invalid annotation for field/property '{0}' on class '{1}': {2}",
                            fieldOrProperty.Name,
                            parent.Name,
                            ex.Message),
                        ex);
                }

                config.Add(child);
            }

            // Methods (setter/getter)
            foreach (var method in typeInfo.DeclaredMethods)
            {
                var ga = method.GetCustomAttribute<GroupAttribute>();
                var ra = method.GetCustomAttribute<RecordAttribute>();
                if (ra == null && ga == null)
                    continue;
                if (ra != null && ga != null)
                    throw new BeanIOConfigurationException(
                        string.Format("Method '{0}' on class '{1}' cannot be annotated with both RecordAttribute and GroupAttribute.", method.Name, parent.Name));

                Type clazz;
                var name = method.Name;
                string getter, setter;

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
                    throw new BeanIOConfigurationException(string.Format("Method '{0}' on class '{1}' is not a valid getter or setter", method.Name, parent.Name));
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
                        child = CreateGroup(info, ga);
                }
                catch (ArgumentException ex)
                {
                    throw new BeanIOConfigurationException(string.Format("Invalid annotation for method '{0}' on class '{1}': {2}", method.Name, parent.Name, ex.Message), ex);
                }

                config.Add(child);
            }
        }

        private static void AddRecordChildren(ComponentConfig config, Type parent)
        {
            var typeInfo = parent.GetTypeInfo();

            var fieldsAndProperties = typeInfo.DeclaredFields.Cast<MemberInfo>().Union(typeInfo.DeclaredProperties);
            foreach (var fieldOrProperty in fieldsAndProperties)
            {
                var fas = fieldOrProperty.GetCustomAttributes<FieldAttribute>().ToList();
                var sa = fieldOrProperty.GetCustomAttribute<SegmentAttribute>();
                if (fas.Count == 0 && sa == null)
                    continue;
                if (fas.Count > 1 && sa == null)
                    throw new BeanIOConfigurationException(
                        string.Format(
                            "Field/Property '{0}' on class '{1}' cannot be annotated with multiple FieldAttribute without SegmentAttribute.",
                            fieldOrProperty.Name,
                            parent.Name));

                var fieldInfo = fieldOrProperty as FieldInfo;
                var propInfo = fieldOrProperty as PropertyInfo;

                var info = new TypeInfo
                {
                    Name = fieldOrProperty.Name,
                    Type = fieldInfo == null ? propInfo.PropertyType : fieldInfo.FieldType,
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
                        string.Format(
                            "Invalid annotation for field/property '{0}' on class '{1}': {2}",
                            fieldOrProperty.Name,
                            parent.Name,
                            ex.Message),
                        ex);
                }

                config.Add(child);
            }

            // Methods (setter/getter)
            foreach (var method in typeInfo.DeclaredMethods)
            {
                var fas = method.GetCustomAttributes<FieldAttribute>().ToList();
                var sa = method.GetCustomAttribute<SegmentAttribute>();
                if (fas.Count == 0 && sa == null)
                    continue;
                if (fas.Count > 1 && sa == null)
                    throw new BeanIOConfigurationException(
                        string.Format(
                            "Method '{0}' on class '{1}' cannot be annotated with multiple FieldAttribute without SegmentAttribute.",
                            method.Name,
                            parent.Name));

                Type clazz;
                var name = method.Name;
                string getter, setter;

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
                    throw new BeanIOConfigurationException(string.Format("Method '{0}' on class '{1}' is not a valid getter or setter", method.Name, parent.Name));
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
                    throw new BeanIOConfigurationException(string.Format("Invalid annotation for method '{0}' on class '{1}': {2}", method.Name, parent.Name, ex.Message), ex);
                }

                config.Add(child);
            }
        }

        private static void UpdateTypeInfo(TypeInfo info, Type annotatedType, Type annotatedCollection)
        {
            annotatedType = annotatedType.ToValue();

            string collectionName;
            var propertyType = info.Type;

            if (propertyType.IsArray)
            {
                propertyType = annotatedType ?? propertyType.GetElementType();
                collectionName = null;
            }
            else if (propertyType.IsInstanceOf(typeof(IDictionary<,>)))
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

                collectionName = collectionType.GetFullName();
            }
            else if (propertyType.IsInstanceOf(typeof(ICollection<>)))
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

                collectionName = collectionType.GetFullName();
            }
            else
            {
                if (annotatedType != null)
                    propertyType = annotatedType;
                collectionName = null;
            }

            info.PropertyType = propertyType;
            info.PropertyName = propertyType.Name;
            info.CollectionName = collectionName;
        }

        private static Type ToValue([CanBeNull] this Type type)
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

        private static string ToValue([CanBeNull] this string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }

        private static int? ToUnboundedValue(this int? n)
        {
            var val = n.ToValue();
            if (val == null)
                return null;
            if (val < 0)
                return int.MaxValue;
            return val.Value;
        }

        private static int? ToUnboundedValue(this int n)
        {
            return ToUnboundedValue((int?)n);
        }

        private class TypeInfo
        {
            public bool IsBound { get; set; }

            public int? ArgumentIndex { get; set; }

            public string Name { get; set; }

            public Type Type { get; set; }

            /// <summary>
            /// Gets or sets the class name of propertyType(?)
            /// </summary>
            public string PropertyName { get; set; }

            public string CollectionName { get; set; }

            public Type PropertyType { get; set; }

            public string Getter { get; set; }

            public string Setter { get; set; }
        }

        private class OrdinalComparer : IComparer<ComponentConfig>, IEqualityComparer<ComponentConfig>
        {
            /// <summary>
            /// Compares the ordinal values of two component configurations.
            /// </summary>
            /// <param name="x">First component configuration</param>
            /// <param name="y">Second component configuration</param>
            /// <returns>&lt;0 when <paramref name="x"/> is smaller than <paramref name="y"/>,
            /// &gt;0 when <paramref name="x"/> is greater than <paramref name="y"/>,
            /// otherwise 0</returns>
            public int Compare(ComponentConfig x, ComponentConfig y)
            {
                return x.Ordinal.GetValueOrDefault().CompareTo(y.Ordinal.GetValueOrDefault());
            }

            /// <summary>
            /// Equality comparison of the ordinal values of two component configurations
            /// </summary>
            /// <param name="x">First component configuration</param>
            /// <param name="y">Second component configuration</param>
            /// <returns>true, when the ordinal values of both component configurations are the same.</returns>
            public bool Equals(ComponentConfig x, ComponentConfig y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                return Compare(x, y) == 0;
            }

            /// <summary>
            /// Calculates the hash code of the ordinal value of a component configuration
            /// </summary>
            /// <param name="obj">The component configuration</param>
            /// <returns>the hash code</returns>
            public int GetHashCode(ComponentConfig obj)
            {
                return obj.Ordinal.GetValueOrDefault().GetHashCode();
            }
        }
    }
}
