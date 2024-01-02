// <copyright file="PropertyDescriptor.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BeanIO.Internal.Util
{
    internal class PropertyDescriptor
    {
        private readonly PropertyInfo? _property;

        private readonly FieldInfo? _field;

        private readonly MethodInfo? _getter;

        private readonly MethodInfo? _setter;

        private Type? _type;

        public PropertyDescriptor(string constructorArgument, MethodInfo? getter = null, MethodInfo? setter = null)
        {
            Name = constructorArgument;
            _field = null;
            _property = null;
            _getter = getter;
            _setter = setter;
        }

        public PropertyDescriptor(PropertyInfo property, MethodInfo? getter = null, MethodInfo? setter = null)
        {
            Name = property.Name;
            _field = null;
            _property = getter != null && setter != null && !ReferenceEquals(property.GetMethod, getter) && !ReferenceEquals(property.SetMethod, setter)
                            ? null
                            : property;
            _getter = getter ?? property.GetMethod;
            _setter = setter ?? property.SetMethod;
        }

        public PropertyDescriptor(FieldInfo field, MethodInfo? getter = null, MethodInfo? setter = null)
        {
            Name = field.Name;
            _field = field;
            _property = null;
            _getter = getter;
            _setter = setter;
        }

        [MemberNotNullWhen(true, nameof(_property), nameof(PropertyInfo))]
        public bool IsProperty => _property != null;

        public bool IsConstructor => _property == null && _field == null;

        [MemberNotNullWhen(true, nameof(_field), nameof(FieldInfo))]
        public bool IsField => _field != null;

        public bool HasGetter => _getter != null || _field != null;

        public bool HasSetter => _setter != null || (_field != null && !_field.IsInitOnly);

        public FieldInfo? FieldInfo => _field;

        public PropertyInfo? PropertyInfo => _property;

        public MethodInfo? GetMethodInfo => _getter;

        public MethodInfo? SetMethodInfo => _setter;

        public string Name { get; }

        public bool? IsPublic
        {
            get
            {
                if (IsField)
                    return _field.IsPublic;
                if (IsProperty)
                    return _getter?.IsPublic ?? _setter?.IsPublic;
                if (_getter != null)
                    return _getter.IsPublic;
                return _setter?.IsPublic;
            }
        }

        public Type PropertyType
        {
            get => _type ??= GuessPropertyType();
            set => _type = value;
        }

        public object? GetValue(object? instance)
        {
            if (_getter != null)
                return _getter.Invoke(instance, null);
            if (IsField)
                return _field.GetValue(instance);
            throw new InvalidOperationException();
        }

        public void SetValue(object? instance, object? value)
        {
            if (_setter != null)
            {
                _setter.Invoke(instance, new[] { value });
            }
            else if (IsField)
            {
                _field.SetValue(instance, value);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private Type GuessPropertyType()
        {
            var setterType = _setter?.GetParameters()[0].ParameterType;
            if (IsField)
                return GetMostSpecificType(_field.FieldType, _getter?.ReturnType, setterType);
            if (IsProperty)
                return GetMostSpecificType(_property.PropertyType, _getter?.ReturnType, setterType);
            return GetMostSpecificType(_getter?.ReturnType, setterType);
        }

        private Type GetMostSpecificType(params Type?[] types)
        {
            var result = typeof(object);
            foreach (var type in types)
            {
                if (type == null)
                    continue;
                if (type != result && type.IsInstanceOf(result))
                    result = type;
            }

            return result;
        }
    }
}
