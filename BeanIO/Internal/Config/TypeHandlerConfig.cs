using System;

using BeanIO.Types;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// Stores configuration settings for a custom type handler.
    /// </summary>
    /// <remarks>
    /// Type handlers are used to convert field text to values and back.
    /// </remarks>
    public class TypeHandlerConfig : BeanConfig<ITypeHandler>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHandlerConfig"/> class.
        /// </summary>
        public TypeHandlerConfig()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHandlerConfig"/> class.
        /// </summary>
        /// <param name="create">the function to create an instance of <see cref="ITypeHandler"/></param>
        public TypeHandlerConfig(Func<ITypeHandler> create)
            : base(create)
        {
        }

        /// <summary>
        /// Gets or sets the name of the type handler.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the class name to register this type handler under.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the stream format to register this type handler for, or null
        /// if the type handler is used for all formats.
        /// </summary>
        public string Format { get; set; }
    }
}
