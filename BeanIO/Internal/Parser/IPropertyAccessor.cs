namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A PropertyAccessor provides access to a bean property.
    /// </summary>
    internal interface IPropertyAccessor
    {
        /// <summary>
        /// Gets a value indicating whether this property is a constructor argument.
        /// </summary>
        bool IsConstructorArgument { get; }

        /// <summary>
        /// Gets the constructor argument index, or null if this property is
        /// not a constructor argument.
        /// </summary>
        int? ConstructorArgumentIndex { get; }

        /// <summary>
        /// Returns the property value from a bean object.
        /// </summary>
        /// <param name="bean">the bean object to get the property from</param>
        /// <returns>the property value</returns>
        object GetValue(object bean);

        /// <summary>
        /// Sets the property value on a bean object.
        /// </summary>
        /// <param name="bean">the bean object to set the property</param>
        /// <param name="value">the property value</param>
        void SetValue(object bean, object value);
    }
}
