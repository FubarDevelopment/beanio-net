using System;
using System.Globalization;
using System.IO;

namespace BeanIO
{
    /// <summary>
    /// A <see cref="StreamFactory"/> is used to load BeanIO mapping files and create
    /// <see cref="IBeanReader"/>, <see cref="IBeanWriter"/>, <see cref="IUnmarshaller"/>
    /// and <see cref="IMarshaller"/> instances.
    /// </summary>
    public abstract class StreamFactory
    {
        /// <summary>
        /// Creates a new <see cref="IBeanReader"/> for reading from the given input stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="input">The input stream to read from</param>
        /// <returns>The new <see cref="IBeanReader"/></returns>
        public virtual IBeanReader CreateReader(string name, Stream input)
        {
            return CreateReader(name, input, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Creates a new <see cref="IBeanReader"/> for reading from the given input stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="input">The input stream to read from</param>
        /// <param name="culture">The culture used to format error messages.</param>
        /// <returns>The new <see cref="IBeanReader"/></returns>
        public abstract IBeanReader CreateReader(string name, Stream input, CultureInfo culture);

        /// <summary>
        /// Creates a new <see cref="IUnmarshaller"/> for unmarshalling records.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <returns>The new <see cref="IUnmarshaller"/></returns>
        public virtual IUnmarshaller CreateUnmarshaller(string name)
        {
            return CreateUnmarshaller(name, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Creates a new <see cref="IUnmarshaller"/> for unmarshalling records.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="culture">The culture used to format error messages.</param>
        /// <returns>The new <see cref="IUnmarshaller"/></returns>
        public abstract IUnmarshaller CreateUnmarshaller(string name, CultureInfo culture);

        /// <summary>
        /// Creates a new <see cref="IBeanWriter"/> for writing to the given output stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="output">The output stream to write to</param>
        /// <returns>The new <see cref="IBeanWriter"/></returns>
        public abstract IBeanWriter CreateWriter(string name, Stream output);

        /// <summary>
        /// Creates a new <see cref="IMarshaller"/> for marshalling records.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <returns>The new <see cref="IMarshaller"/></returns>
        public abstract IMarshaller CreateMarshaller(string name);

#if FALSE
        /// <summary>
        /// Defines a new stream mapping.
        /// </summary>
        /// <param name="builder">The <see cref="StreamBuilder"/>.</param>
        public abstract void Define(StreamBuilder builder);
#endif

        /// <summary>
        /// Loads a BeanIO mapping file, and adds the configured streams to this factory.
        /// </summary>
        /// <param name="input">The input stream to read the mapping file from</param>
        public abstract void Load(Stream input);

        /// <summary>
        /// Test whether a mapping configuration exists for a named stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <returns>true if a mapping configuration is found for the named stream</returns>
        public abstract bool IsMapped(string name);

        /// <summary>
        /// This method is invoked after a StreamFactory is loaded and all attributes have been set.
        /// </summary>
        protected virtual void Init()
        {
        }
    }
}
