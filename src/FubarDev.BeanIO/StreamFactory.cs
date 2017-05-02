// <copyright file="StreamFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using BeanIO.Builder;
using BeanIO.Config;
using BeanIO.Internal.Util;

namespace BeanIO
{
    /// <summary>
    /// A <see cref="StreamFactory"/> is used to load BeanIO mapping files and create
    /// <see cref="IBeanReader"/>, <see cref="IBeanWriter"/>, <see cref="IUnmarshaller"/>
    /// and <see cref="IMarshaller"/> instances.
    /// </summary>
    public abstract class StreamFactory : IStreamFactory
    {
        protected StreamFactory(ISettings settings, ISchemeProvider schemeProvider)
        {
            Settings = settings;
            SchemeProvider = schemeProvider;
        }

        /// <summary>
        /// Gets the <see cref="ISettings"/> used to configure this <see cref="IStreamFactory"/>
        /// </summary>
        public ISettings Settings { get; }

        /// <summary>
        /// Gets the <see cref="ISchemeProvider"/> used to load the referenced resources
        /// </summary>
        public ISchemeProvider SchemeProvider { get; }

        /// <summary>
        /// Creates a new instance of the configured <see cref="StreamFactory"/>.
        /// </summary>
        /// <returns>The new <see cref="StreamFactory"/></returns>
        /// <exception cref="BeanIOConfigurationException">Will be thrown when the <see cref="StreamFactory"/>
        /// instance isn't configured or cannot be instantiated</exception>
        public static IStreamFactory NewInstance()
        {
            var settings = DefaultConfigurationFactory.CreateDefaultSettings();
            var schemeProvider = DefaultConfigurationFactory.CreateDefaultSchemeProvider();
            return NewInstance(settings, schemeProvider);
        }

        /// <summary>
        /// Creates a new instance of the configured <see cref="StreamFactory"/>.
        /// </summary>
        /// <param name="settings">The settings used to configure the returned <see cref="IStreamFactory"/></param>
        /// <param name="schemeProvider">The scheme provider used to load the resources</param>
        /// <returns>The new <see cref="StreamFactory"/></returns>
        /// <exception cref="BeanIOConfigurationException">Will be thrown when the <see cref="StreamFactory"/>
        /// instance isn't configured or cannot be instantiated</exception>
        public static IStreamFactory NewInstance(ISettings settings, ISchemeProvider schemeProvider)
        {
            var className = settings[ConfigurationKeys.STREAM_FACTORY_CLASS];
            if (string.IsNullOrEmpty(className))
                throw new BeanIOConfigurationException($"Property '{ConfigurationKeys.STREAM_FACTORY_CLASS}' not set");

            try
            {
                var arguments = new List<object>() { settings, schemeProvider };
                var factory = (IStreamFactory)Type.GetType(className).NewInstance(arguments);
                var abstractFactory = factory as StreamFactory;
                abstractFactory?.Init();
                return factory;
            }
            catch (Exception ex)
            {
                throw new BeanIOConfigurationException($"Failed to load stream factory implementation class '{className}'", ex);
            }
        }

        /// <summary>
        /// Creates a new <see cref="IBeanReader"/> for reading from the given input stream.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <param name="input">The input stream to read from</param>
        /// <returns>The new <see cref="IBeanReader"/></returns>
        public virtual IBeanReader CreateReader(string name, TextReader input)
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
        public abstract IBeanReader CreateReader(string name, TextReader input, CultureInfo culture);

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
        public abstract IBeanWriter CreateWriter(string name, TextWriter output);

        /// <summary>
        /// Creates a new <see cref="IMarshaller"/> for marshalling records.
        /// </summary>
        /// <param name="name">The name of the stream in the mapping file</param>
        /// <returns>The new <see cref="IMarshaller"/></returns>
        public abstract IMarshaller CreateMarshaller(string name);

        /// <summary>
        /// Defines a new stream mapping.
        /// </summary>
        /// <param name="builder">The <see cref="StreamBuilder"/>.</param>
        public abstract void Define(StreamBuilder builder);

        /// <summary>
        /// Loads a BeanIO mapping file, and adds the configured streams to this factory.
        /// </summary>
        /// <param name="input">The input stream to read the mapping file from</param>
        public virtual void Load(System.IO.Stream input)
        {
            Load(input, null);
        }

        /// <summary>
        /// Loads a BeanIO mapping, and adds the configured streams to this factory.
        /// </summary>
        /// <param name="source">The source to read the mapping file from</param>
        public virtual void Load(Uri source)
        {
            Load(source, null);
        }

        /// <summary>
        /// Loads a BeanIO mapping file, and adds the configured streams to this factory.
        /// </summary>
        /// <param name="source">The source to read the mapping file from</param>
        /// <param name="properties">user <see cref="Properties"/> for property substitution</param>
        public virtual void Load(Uri source, Properties properties)
        {
            var handler = SchemeProvider.GetSchemeHandler(source.Scheme, true);
            using (var input = handler.Open(source))
                Load(input, properties);
        }

        /// <summary>
        /// Loads a BeanIO mapping file, and adds the configured streams to this factory.
        /// </summary>
        /// <param name="input">The input stream to read the mapping file from</param>
        /// <param name="properties">user <see cref="Properties"/> for property substitution</param>
        public abstract void Load(System.IO.Stream input, Properties properties);

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
