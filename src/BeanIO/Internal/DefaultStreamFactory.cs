// <copyright file="DefaultStreamFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Concurrent;
using System.Globalization;
using System.IO;

using BeanIO.Builder;
using BeanIO.Config;
using BeanIO.Internal.Compiler;

namespace BeanIO.Internal
{
    /// <summary>
    /// The <see cref="DefaultStreamFactory"/> stores configured stream definitions used
    /// to create bean readers and writers.  A single factory instance may be accessed
    /// concurrently by multiple threads.
    /// </summary>
    internal class DefaultStreamFactory : StreamFactory
    {
        private readonly ConcurrentDictionary<string, Parser.Stream> _contextMap = new ConcurrentDictionary<string, Parser.Stream>();

        /// <summary>
        /// Gets or sets the mapping compiler to use for compiling streams
        /// </summary>
        public StreamCompiler Compiler { get; set; }

        /// <summary>
        /// Adds a stream to this manager
        /// </summary>
        /// <param name="stream">the <see cref="Parser.Stream"/> to add</param>
        public void AddStream(Parser.Stream stream)
        {
            _contextMap[stream.Name] = stream;
        }

        /// <summary>
        /// Removes the named stream from this manager
        /// </summary>
        /// <param name="name">the name of the stream to remove</param>
        /// <returns>the removed <see cref="Parser.Stream"/>, or <code>null</code> if
        /// the there was no stream for the given name</returns>
        public Parser.Stream RemoveStream(string name)
        {
            Parser.Stream result;
            if (_contextMap.TryRemove(name, out result))
                return result;
            return null;
        }

        /// <summary>
        /// Creates a new <see cref="IBeanReader"/> for reading from the given input stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="input">The input stream to read from</param>
        /// <param name="culture">The culture used to format error messages.</param>
        /// <returns>The new <see cref="IBeanReader"/></returns>
        public override IBeanReader CreateReader(string name, TextReader input, CultureInfo culture)
        {
            if (culture == null)
                culture = CultureInfo.CurrentCulture;

            var stream = GetStream(name);
            switch (stream.Mode)
            {
                case AccessMode.ReadWrite:
                case AccessMode.Read:
                    return stream.CreateBeanReader(input, culture);
                default:
                    throw new BeanIOException($"Write mode not supported for stream mapping '{name}'");
            }
        }

        /// <summary>
        /// Creates a new <see cref="IUnmarshaller"/> for unmarshalling records.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="culture">The culture used to format error messages.</param>
        /// <returns>The new <see cref="IUnmarshaller"/></returns>
        public override IUnmarshaller CreateUnmarshaller(string name, CultureInfo culture)
        {
            if (culture == null)
                culture = CultureInfo.CurrentCulture;

            var stream = GetStream(name);
            switch (stream.Mode)
            {
                case AccessMode.ReadWrite:
                case AccessMode.Read:
                    return stream.CreateUnmarshaller(culture);
                default:
                    throw new BeanIOException($"Write mode not supported for stream mapping '{name}'");
            }
        }

        /// <summary>
        /// Creates a new <see cref="IBeanWriter"/> for writing to the given output stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="output">The output stream to write to</param>
        /// <returns>The new <see cref="IBeanWriter"/></returns>
        public override IBeanWriter CreateWriter(string name, TextWriter output)
        {
            var stream = GetStream(name);
            switch (stream.Mode)
            {
                case AccessMode.ReadWrite:
                case AccessMode.Write:
                    return stream.CreateBeanWriter(output);
                default:
                    throw new BeanIOException($"Read mode not supported for stream mapping '{name}'");
            }
        }

        /// <summary>
        /// Creates a new <see cref="IMarshaller"/> for marshalling records.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <returns>The new <see cref="IMarshaller"/></returns>
        public override IMarshaller CreateMarshaller(string name)
        {
            var stream = GetStream(name);
            switch (stream.Mode)
            {
                case AccessMode.ReadWrite:
                case AccessMode.Write:
                    return stream.CreateMarshaller();
                default:
                    throw new BeanIOException($"Read mode not supported for stream mapping '{name}'");
            }
        }

        /// <summary>
        /// Defines a new stream mapping.
        /// </summary>
        /// <param name="builder">The <see cref="StreamBuilder"/>.</param>
        public override void Define(StreamBuilder builder)
        {
            AddStream(Compiler.Build(builder.Build()));
        }

        /// <summary>
        /// Loads a BeanIO mapping file, and adds the configured streams to this factory.
        /// </summary>
        /// <param name="input">The input stream to read the mapping file from</param>
        /// <param name="properties">user <see cref="Properties"/> for property substitution</param>
        public override void Load(System.IO.Stream input, Properties properties)
        {
            var streams = Compiler.LoadMapping(input, properties);
            foreach (var stream in streams)
                AddStream(stream);
        }

        /// <summary>
        /// Test whether a mapping configuration exists for a named stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <returns>true if a mapping configuration is found for the named stream</returns>
        public override bool IsMapped(string name)
        {
            return _contextMap.ContainsKey(name);
        }

        /// <summary>
        /// This method is invoked after a StreamFactory is loaded and all attributes have been set.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            Compiler = new StreamCompiler();
        }

        /// <summary>
        /// Returns the named stream
        /// </summary>
        /// <param name="name">the name of the stream</param>
        /// <returns>the <see cref="Parser.Stream"/></returns>
        protected Parser.Stream GetStream(string name)
        {
            Parser.Stream stream;
            if (!_contextMap.TryGetValue(name, out stream))
                throw new BeanIOException($"No stream mapping configured for name '{name}'");
            return stream;
        }
    }
}
