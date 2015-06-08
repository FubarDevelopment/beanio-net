using System;
using System.Reflection;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Accessor
{
    /// <summary>
    /// A <see cref="IPropertyAccessor"/> that uses reflection to access a public field.
    /// </summary>
    public class FieldReflectionAccessor : PropertyAccessorSupport
    {
        private readonly FieldInfo _field;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldReflectionAccessor"/> class.
        /// </summary>
        /// <param name="field">the reflected <see cref="FieldInfo"/></param>
        /// <param name="constructorArgumentIndex">the constructor argument index, or null if not a constructor argument</param>
        public FieldReflectionAccessor(FieldInfo field, int? constructorArgumentIndex)
        {
            ConstructorArgumentIndex = constructorArgumentIndex;
            _field = field;
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
                return _field.GetValue(bean);
            }
            catch (Exception ex)
            {
                throw new BeanIOException(string.Format("Failed to get field '{0}' from bean class '{1}'", _field.Name, bean.GetType().GetAssemblyQualifiedName()), ex);
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
                _field.SetValue(bean, value);
            }
            catch (Exception ex)
            {
                throw new BeanIOException(string.Format("Failed to set field '{0}' on bean class '{1}': {2}", _field.Name, bean.GetType().GetAssemblyQualifiedName(), ex.Message), ex);
            }
        }
    }
}
