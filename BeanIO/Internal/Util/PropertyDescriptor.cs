using System;
using System.Reflection;

using JetBrains.Annotations;

namespace BeanIO.Internal.Util
{
    internal class PropertyDescriptor
    {
        private readonly string _name;

        private readonly PropertyInfo _property;

        private readonly FieldInfo _field;

        private readonly MethodInfo _getter;

        private readonly MethodInfo _setter;

        private Type _type;

        public PropertyDescriptor([NotNull] string constructorArgument, MethodInfo getter = null, MethodInfo setter = null)
        {
            _name = constructorArgument;
            _field = null;
            _property = null;
            _getter = getter;
            _setter = setter;
        }

        public PropertyDescriptor([NotNull] PropertyInfo property, MethodInfo getter = null, MethodInfo setter = null)
        {
            _name = property.Name;
            _field = null;
            _property = property;
            _getter = getter ?? property.GetMethod;
            _setter = setter ?? property.SetMethod;
        }

        public PropertyDescriptor([NotNull] FieldInfo field, MethodInfo getter = null, MethodInfo setter = null)
        {
            _name = field.Name;
            _field = field;
            _property = null;
            _getter = getter;
            _setter = setter;
        }

        public bool HasGetter
        {
            get { return _getter != null || _field != null; }
        }

        public bool HasSetter
        {
            get { return _setter != null || (_field != null && !_field.IsInitOnly); }
        }

        public string Name
        {
            get { return _name; }
        }

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
                if (_setter != null)
                    return _setter.IsPublic;
                return null;
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
            if (_field != null)
                return _field.FieldType;
            if (_property != null)
                return _property.PropertyType;
            if (_getter != null)
                return _getter.ReturnType;
            if (_setter != null)
                return _setter.GetParameters()[0].ParameterType;
            return null;
        }
    }
}
