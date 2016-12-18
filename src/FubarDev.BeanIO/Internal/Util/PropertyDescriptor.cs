// <copyright file="PropertyDescriptor.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;

using JetBrains.Annotations;

namespace BeanIO.Internal.Util
{
    internal class PropertyDescriptor
    {
        private readonly PropertyInfo _property;

        private readonly FieldInfo _field;

        private readonly MethodInfo _getter;

        private readonly MethodInfo _setter;

        private Type _type;

        public PropertyDescriptor([NotNull] string constructorArgument, MethodInfo getter = null, MethodInfo setter = null)
        {
            Name = constructorArgument;
            _field = null;
            _property = null;
            _getter = getter;
            _setter = setter;
        }

        public PropertyDescriptor([NotNull] PropertyInfo property, MethodInfo getter = null, MethodInfo setter = null)
        {
            Name = property.Name;
            _field = null;
            _property = getter != null && setter != null && !ReferenceEquals(property.GetMethod, getter) && !ReferenceEquals(property.SetMethod, setter)
                            ? null
                            : property;
            _getter = getter ?? property.GetMethod;
            _setter = setter ?? property.SetMethod;
        }

        public PropertyDescriptor([NotNull] FieldInfo field, MethodInfo getter = null, MethodInfo setter = null)
        {
            Name = field.Name;
            _field = field;
            _property = null;
            _getter = getter;
            _setter = setter;
        }

        public bool HasGetter => _getter != null || _field != null;

        public bool HasSetter => _setter != null || (_field != null && !_field.IsInitOnly);

        public FieldInfo FieldInfo => _field;

        public PropertyInfo PropertyInfo => _property;

        public MethodInfo GetMethodInfo => _getter;

        public MethodInfo SetMethodInfo => _setter;

        public string Name { get; }

        public bool? IsPublic
        {
            get
            {
                if (_field != null)
                    return _field.IsPublic;
                if (_property != null)
                    return _getter.IsPublic;
                if (_getter != null)
                    return _getter.IsPublic;
                return _setter?.IsPublic;
            }
        }

        public Type PropertyType
        {
            get { return _type ?? (_type = GuessPropertyType()); }
            set { _type = value; }
        }

        public object GetValue(object instance)
        {
            if (_getter != null)
                return _getter.Invoke(instance, null);
            if (_field != null)
                return _field.GetValue(instance);
            throw new InvalidOperationException();
        }

        public void SetValue(object instance, object value)
        {
            if (_setter != null)
            {
                _setter.Invoke(instance, new[] { value });
            }
            else if (_field != null)
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
            if (_field != null)
                return GetMostSpecificType(_field.FieldType, _getter?.ReturnType, setterType);
            if (_property != null)
                return GetMostSpecificType(_property.PropertyType, _getter?.ReturnType, setterType);
            return GetMostSpecificType(_getter?.ReturnType, setterType);
        }

        private Type GetMostSpecificType(params Type[] types)
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
