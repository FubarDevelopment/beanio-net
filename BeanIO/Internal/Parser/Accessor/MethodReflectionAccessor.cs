using System;
using System.Reflection;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Accessor
{
    public class MethodReflectionAccessor : PropertyAccessorSupport
    {
        private readonly MethodInfo _getter;
        private readonly MethodInfo _setter;

        public MethodReflectionAccessor(MethodInfo getter, MethodInfo setter, int? constructorArgumentIndex)
        {
            ConstructorArgumentIndex = constructorArgumentIndex;
            _getter = getter;
            _setter = setter;
        }

        /// <summary>
        /// Returns the property value from a bean object.
        /// </summary>
        /// <param name="bean">the bean object to get the property from</param>
        /// <returns>the property value</returns>
        public override object GetValue(object bean)
        {
            if (_getter == null)
                throw new BeanIOException(string.Format("There is no getter defined on bean class '{0}'", bean.GetType().GetFullName()));

            try
            {
                return _getter.Invoke(bean, null);
            }
            catch (Exception ex)
            {
                throw new BeanIOException(string.Format("Failed to invoke method '{0}' on bean class '{1}': {2}", _getter.Name, bean.GetType().GetFullName(), ex.Message), ex);
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
                throw new BeanIOException(string.Format("There is no setter defined on bean class '{0}'", bean.GetType().GetFullName()));

            try
            {
                _setter.Invoke(bean, new[] { value });
            }
            catch (Exception ex)
            {
                throw new BeanIOException(string.Format("Failed to invoke method '{0}' on bean class '{1}': {2}", _setter.Name, bean.GetType().GetFullName(), ex.Message), ex);
            }
        }
    }
}
