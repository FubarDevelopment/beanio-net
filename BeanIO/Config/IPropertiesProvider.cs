using System.Collections.Generic;

namespace BeanIO.Config
{
    /// <summary>
    /// Property provider interface
    /// </summary>
    public interface IPropertiesProvider
    {
        /// <summary>
        /// Reads all properties
        /// </summary>
        /// <returns>A dictionary with all properties read</returns>
        IReadOnlyDictionary<string, string> Read();
    }
}
