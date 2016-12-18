// <copyright file="InlineMappingLoader.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Config;
using BeanIO.Internal.Config;
using BeanIO.Internal.Config.Xml;

namespace BeanIO.Builder
{
    public class InlineMappingLoader<T, TConfig>
        where T : SegmentBuilderSupport<T, TConfig>
        where TConfig : SegmentConfig
    {
        private readonly XmlMappingParser _mappingParser;

        private readonly ISchemeProvider _schemeProvider;

        private readonly TConfig _config;

        internal InlineMappingLoader(
            SegmentBuilderSupport<T, TConfig> builder,
            ISettings settings,
            ISchemeProvider schemeProvider)
        {
            Builder = builder;
            _config = builder.Config;
            _schemeProvider = schemeProvider;
            _mappingParser = new XmlMappingParser(settings, schemeProvider, new XmlMappingReader());
        }

        public SegmentBuilderSupport<T, TConfig> Builder { get; }

        /// <summary>
        /// Loads the data from a resource whose templates can be accessed using <see cref="Include"/>.
        /// </summary>
        /// <param name="resource">The resource to load that gives access to a template</param>
        /// <param name="properties">The optional properties to use while loading data from the resource</param>
        /// <returns>The value of <see cref="P:SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public InlineMappingLoader<T, TConfig> LoadMapping(Uri resource, Properties properties = null)
        {
            var handler = _schemeProvider.GetSchemeHandler(resource.Scheme, true);
            using (var input = handler.Open(resource))
                _mappingParser.LoadConfiguration(input, properties);
            return this;
        }

        /// <summary>
        /// Includes the data from a given template
        /// </summary>
        /// <param name="templateName">The name of the template to apply to this builder.</param>
        /// <param name="offset">the value to offset configured positions by</param>
        /// <returns>The value of <see cref="P:SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public InlineMappingLoader<T, TConfig> Include(string templateName, int offset = 0)
        {
            _mappingParser.IncludeTemplate(_config, templateName, offset);
            return this;
        }
    }
}
