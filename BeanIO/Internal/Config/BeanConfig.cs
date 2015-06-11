using System;

using BeanIO.Config;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// Stores bean information.
    /// </summary>
    /// <remarks>
    /// A <code>Bean</code> object is used to instantiate configurable components
    /// such as a type handler, record reader factory or record writer factory.
    /// </remarks>
    /// <typeparam name="T">The bean type</typeparam>
    public class BeanConfig<T> : IBeanConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeanConfig{T}"/> class.
        /// </summary>
        public BeanConfig()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanConfig{T}"/> class.
        /// </summary>
        /// <param name="create">the function to create an instance of <typeparamref name="T"/></param>
        public BeanConfig(Func<T> create)
        {
            Create = create;
        }

        /// <summary>
        /// Gets or sets the fully qualified class name of the bean.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets the bean properties.
        /// </summary>
        public Properties Properties { get; set; }

        /// <summary>
        /// Gets or sets the configured bean instance.
        /// </summary>
        public Func<T> Create { get; private set; }
    }
}
