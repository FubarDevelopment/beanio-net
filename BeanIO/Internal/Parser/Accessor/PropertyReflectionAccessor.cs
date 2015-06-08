﻿using System;
using System.Reflection;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Accessor
{
    public class PropertyReflectionAccessor : PropertyAccessorSupport
    {
        private readonly PropertyDescriptor _property;

        public PropertyReflectionAccessor(PropertyDescriptor property, int? constructorArgumentIndex)
        {
            ConstructorArgumentIndex = constructorArgumentIndex;
            _property = property;
        }

        /// <summary>
        /// Returns the property value from a bean object.
        /// </summary>
        /// <param name="bean">the bean object to get the property from</param>
        /// <returns>the property value</returns>
        public override object GetValue(object bean)
        {
            try
            {
                return _property.GetValue(bean);
            }
            catch (Exception ex)
            {
                throw new BeanIOException(
                    string.Format(
                        "Failed to get value for property or field '{2}' on bean class '{0}': {1}",
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
            try
            {
                _property.SetValue(bean, value);
            }
            catch (Exception ex)
            {
                throw new BeanIOException(
                    string.Format(
                        "Failed to set value for property or field '{2}' on bean class '{0}': {1}",
                        bean.GetType().GetFullName(),
                        ex.Message,
                        _property.Name),
                    ex);
            }
        }
    }
}
