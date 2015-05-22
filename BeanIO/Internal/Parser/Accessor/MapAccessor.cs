using System.Collections;

namespace BeanIO.Internal.Parser.Accessor
{
    public class MapAccessor : IPropertyAccessor
    {
        private readonly string _key;

        public MapAccessor(string key)
        {
            _key = key;
        }

        /// <summary>
        /// Gets a value indicating whether this property is a constructor argument.
        /// </summary>
        public bool IsConstructorArgument
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the constructor argument index, or null if this property is
        /// not a constructor argument.
        /// </summary>
        public int? ConstructorArgumentIndex
        {
            get { return null; }
        }

        /// <summary>
        /// Returns the property value from a bean object.
        /// </summary>
        /// <param name="bean">the bean object to get the property from</param>
        /// <returns>the property value</returns>
        public object GetValue(object bean)
        {
            return ((IDictionary)bean)[_key];
        }

        /// <summary>
        /// Sets the property value on a bean object.
        /// </summary>
        /// <param name="bean">the bean object to set the property</param>
        /// <param name="value">the property value</param>
        public void SetValue(object bean, object value)
        {
            ((IDictionary)bean)[_key] = value;
        }
    }
}
