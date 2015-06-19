using System;

using Newtonsoft.Json.Linq;

namespace BeanIO.Internal.Parser.Format.Json
{
    /// <summary>
    /// An interface implemented by any JSON segment or field.
    /// </summary>
    public interface IJsonNode
    {
        /// <summary>
        /// Gets the field name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the JSON field name.
        /// </summary>
        string JsonName { get; }

        /// <summary>
        /// Gets the type of node.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsJsonArray"/> is true, this method returns the component type of the array.
        /// </remarks>
        JTokenType JsonType { get; }

        /// <summary>
        /// Gets a value indicating whether this node is a JSON array.
        /// </summary>
        bool IsJsonArray { get; }

        /// <summary>
        /// Gets the index of this node in its parent array, or -1 if not applicable
        /// (i.e. its parent is an object).
        /// </summary>
        int JsonArrayIndex { get; }

        /// <summary>
        /// Gets a value indicating whether whether this node may be explicitly set to <code>null</code>.
        /// </summary>
        bool IsNillable { get; }
    }
}
