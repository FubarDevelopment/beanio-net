using BeanIO.Types;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// A base class for configuration components that can be bound to "simple
    /// attributes" of a bean object.  A simple attribute is one that does not have
    /// any child properties itself and can be derived using a <see cref="ITypeHandler"/>.
    /// </summary>
    public abstract class SimplePropertyConfig : PropertyConfig
    {
        /// <summary>
        /// Gets or sets the name of the custom type handler used for type
        /// conversion by this component, or <code>null</code> if the default
        /// type handler is sufficient.
        /// </summary>
        public string TypeHandler { get; set; }

        /// <summary>
        /// Gets or sets the type handler instance used for type conversion
        /// by this component, or null if the default type handler is sufficient.
        /// </summary>
        public ITypeHandler TypeHandlerInstance { get; set; }

        /// <summary>
        /// Gets or sets the pattern used by date and number type handlers to parse
        /// and format the property value.
        /// </summary>
        public string Format { get; set; }
    }
}
