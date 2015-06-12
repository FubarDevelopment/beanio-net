namespace BeanIO.Internal.Util
{
    /// <summary>
    /// A source of property values.
    /// </summary>
    internal interface IPropertySource
    {
        /// <summary>
        /// Returns the property value for a given key
        /// </summary>
        /// <param name="key">the property key</param>
        /// <returns>the property value</returns>
        string GetProperty(string key);
    }
}
