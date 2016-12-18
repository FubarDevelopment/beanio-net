// <copyright file="IStreamFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Globalization;
using System.IO;
using BeanIO.Builder;
using BeanIO.Config;

namespace BeanIO
{
    /// <summary>
    /// A <see cref="IStreamFactory"/> is used to load BeanIO mapping files and create
    /// <see cref="IBeanReader"/>, <see cref="IBeanWriter"/>, <see cref="IUnmarshaller"/>
    /// and <see cref="IMarshaller"/> instances.
    /// </summary>
    public interface IStreamFactory
    {
        /// <summary>
        /// Gets the <see cref="ISettings"/> used to configure this <see cref="IStreamFactory"/>
        /// </summary>
        ISettings Settings { get; }

        /// <summary>
        /// Gets the <see cref="ISchemeProvider"/> used to load the referenced resources
        /// </summary>
        ISchemeProvider SchemeProvider { get; }

        /// <summary>
        /// Creates a new <see cref="IBeanReader"/> for reading from the given input stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="input">The input stream to read from</param>
        /// <returns>The new <see cref="IBeanReader"/></returns>
        IBeanReader CreateReader(string name, TextReader input);

        /// <summary>
        /// Creates a new <see cref="IBeanReader"/> for reading from the given input stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="input">The input stream to read from</param>
        /// <param name="culture">The culture used to format error messages.</param>
        /// <returns>The new <see cref="IBeanReader"/></returns>
        IBeanReader CreateReader(string name, TextReader input, CultureInfo culture);

        /// <summary>
        /// Creates a new <see cref="IUnmarshaller"/> for unmarshalling records.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <returns>The new <see cref="IUnmarshaller"/></returns>
        IUnmarshaller CreateUnmarshaller(string name);

        /// <summary>
        /// Creates a new <see cref="IUnmarshaller"/> for unmarshalling records.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="culture">The culture used to format error messages.</param>
        /// <returns>The new <see cref="IUnmarshaller"/></returns>
        IUnmarshaller CreateUnmarshaller(string name, CultureInfo culture);

        /// <summary>
        /// Creates a new <see cref="IBeanWriter"/> for writing to the given output stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="output">The output stream to write to</param>
        /// <returns>The new <see cref="IBeanWriter"/></returns>
        IBeanWriter CreateWriter(string name, TextWriter output);

        /// <summary>
        /// Creates a new <see cref="IMarshaller"/> for marshalling records.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <returns>The new <see cref="IMarshaller"/></returns>
        IMarshaller CreateMarshaller(string name);

        /// <summary>
        /// Defines a new stream mapping.
        /// </summary>
        /// <param name="builder">The <see cref="StreamBuilder"/>.</param>
        void Define(StreamBuilder builder);

        /// <summary>
        /// Loads a BeanIO mapping file, and adds the configured streams to this factory.
        /// </summary>
        /// <param name="input">The input stream to read the mapping file from</param>
        void Load(System.IO.Stream input);

        /// <summary>
        /// Loads a BeanIO mapping, and adds the configured streams to this factory.
        /// </summary>
        /// <param name="source">The source to read the mapping file from</param>
        void Load(Uri source);

        /// <summary>
        /// Loads a BeanIO mapping file, and adds the configured streams to this factory.
        /// </summary>
        /// <param name="source">The source to read the mapping file from</param>
        /// <param name="properties">user <see cref="Properties"/> for property substitution</param>
        void Load(Uri source, Properties properties);

        /// <summary>
        /// Loads a BeanIO mapping file, and adds the configured streams to this factory.
        /// </summary>
        /// <param name="input">The input stream to read the mapping file from</param>
        /// <param name="properties">user <see cref="Properties"/> for property substitution</param>
        void Load(System.IO.Stream input, Properties properties);

        /// <summary>
        /// Test whether a mapping configuration exists for a named stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <returns>true if a mapping configuration is found for the named stream</returns>
        bool IsMapped(string name);
    }
}
