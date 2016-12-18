// <copyright file="XmlConfigurationLoader.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using BeanIO.Config;

namespace BeanIO.Internal.Config.Xml
{
    /// <summary>
    /// Loads BeanIO mapping files in XML format
    /// </summary>
    /// <remarks>
    /// This class is made thread safe by delegating most of the parsing logic
    /// to <see cref="XmlMappingParser"/>, for which a new instance is created for
    /// each input stream that requires parsing.
    /// </remarks>
    internal class XmlConfigurationLoader : IConfigurationLoader
    {
        private readonly ISettings _settings;
        private readonly ISchemeProvider _schemeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlConfigurationLoader"/> class.
        /// </summary>
        public XmlConfigurationLoader(ISettings settings, ISchemeProvider schemeProvider)
        {
            _settings = settings;
            _schemeProvider = schemeProvider;
            Reader = new XmlMappingReader();
        }

        /// <summary>
        /// Gets the <see cref="XmlMappingReader"/> for reading XML mapping files into a document object model (DOM).
        /// </summary>
        protected virtual XmlMappingReader Reader { get; }

        /// <summary>
        /// Loads a BeanIO configuration from an input stream
        /// </summary>
        /// <param name="input">the input stream to read the configuration from</param>
        /// <param name="properties">the <see cref="Properties"/> for expansion in the mapping file</param>
        /// <returns>a collection of loaded BeanIO configurations</returns>
        public ICollection<BeanIOConfig> LoadConfiguration(System.IO.Stream input, Properties properties)
        {
            return CreateParser().LoadConfiguration(input, properties);
        }

        /// <summary>
        /// Creates a <see cref="XmlMappingParser"/> for reading an mapping input stream
        /// </summary>
        /// <returns>a new XML mapping parser</returns>
        protected virtual XmlMappingParser CreateParser()
        {
            return new XmlMappingParser(_settings, _schemeProvider, Reader);
        }
    }
}
