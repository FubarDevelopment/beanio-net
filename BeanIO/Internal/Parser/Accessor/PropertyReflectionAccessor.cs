using System;
using System.Reflection;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Accessor
{
    public class PropertyReflectionAccessor : PropertyAccessorSupport
    {
        private readonly PropertyInfo _property;
        private readonly MethodInfo _getter;
        private readonly MethodInfo _setter;

        public PropertyReflectionAccessor(PropertyInfo property, MethodInfo getter, MethodInfo setter, int? constructorArgumentIndex)
        {
            ConstructorArgumentIndex = constructorArgumentIndex;
            _property = property;
            _getter = getter ?? property.GetMethod;
            _setter = setter ?? property.SetMethod;
        }

        /// <summary>
        /// Returns the property value from a bean object.
        /// </summary>
        /// <param name="bean">the bean object to get the property from</param>
        /// <returns>the property value</returns>
        public override object GetValue(object bean)
        {
            if (_getter == null)
                throw new BeanIOException(
                    string.Format(
                        "There is no getter defined for property '{1}' on bean class '{0}'",
                        bean.GetType().GetFullName(),
                        _property.Name));

            try
            {
                return _getter.Invoke(bean, null);
            }
            catch (Exception ex)
            {
                throw new BeanIOException(
                    string.Format(
                        "Failed to invoke getter '{0}' for property '{3}' on bean class '{1}': {2}",
                        _getter.Name,
                        bean.GetType().GetFullName(),
                        ex.Message,
                        _property.Name),
                    ex);
            }
        }

        /// <summary>
        /// Sets the property value on a bean object.
        /// </summary>
        /// <param name="bean">the bean object to set the property</param>
        /// <param name="value">the property value</param>
        public override void SetValue(object bean, object value)
        {
            if (_setter == null)
                throw new BeanIOException(
                    string.Format(
                        "There is no setter defined for property '{1}' on bean class '{0}'",
                        bean.GetType().GetFullName(),
                        _property.Name));

            try
            {
                _setter.Invoke(bean, new[] { value });
            }
            catch (Exception ex)
            {
                throw new BeanIOException(
                    string.Format(
                        "Failed to invoke setter '{0}' for property '{3}' on bean class '{1}': {2}",
                        _setter.Name,
                        bean.GetType().GetFullName(),
                        ex.Message,
                        _property.Name),
                    ex);
            }
        }
    }
}
