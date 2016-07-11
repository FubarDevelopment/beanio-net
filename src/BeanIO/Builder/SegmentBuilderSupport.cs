// <copyright file="SegmentBuilderSupport.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Config;
using BeanIO.Internal.Config;
using BeanIO.Internal.Config.Xml;
using BeanIO.Internal.Util;

namespace BeanIO.Builder
{
    /// <summary>
    /// Support for segment configuration builders.
    /// </summary>
    /// <typeparam name="T">The class derived from <see cref="SegmentBuilderSupport{T,TConfig}"/>.</typeparam>
    /// <typeparam name="TConfig">The configuration class derived from <see cref="SegmentConfig"/>.</typeparam>
    public abstract class SegmentBuilderSupport<T, TConfig> : PropertyBuilderSupport<T, TConfig>
        where T : SegmentBuilderSupport<T, TConfig>
        where TConfig : SegmentConfig
    {
        private XmlMappingParser _mappingParser;

        internal XmlMappingParser MappingParser => _mappingParser ?? (_mappingParser = new XmlMappingParser(new XmlMappingReader()));

        /// <summary>
        /// Sets the name of a child component to use as the key for an
        /// inline map bound to this record or segment.
        /// </summary>
        /// <param name="name">the component name</param>
        /// <returns>The value of <see cref="P:SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T Key(string name)
        {
            Config.SetKey(name);
            return Me;
        }

        /// <summary>
        /// Sets the name of a child component to return as the value for this
        /// record or segment in lieu of a bound class.
        /// </summary>
        /// <param name="name">the component name</param>
        /// <returns>The value of <see cref="P:SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T Value(string name)
        {
            Config.Target = name;
            return Me;
        }

        /// <summary>
        /// Adds a segment to this component.
        /// </summary>
        /// <param name="segment">the segment to add</param>
        /// <returns>The value of <see cref="P:SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T AddSegment(SegmentBuilder segment)
        {
            Config.Add(segment.Build());
            return Me;
        }

        /// <summary>
        /// Adds a field to this component.
        /// </summary>
        /// <param name="field">the field to add</param>
        /// <returns>The value of <see cref="P:SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T AddField(FieldBuilder field)
        {
            Config.Add(field.Build());
            return Me;
        }

        /// <summary>
        /// Resets the mapping parser which allows loading a fresh set of mappings (and templates)
        /// </summary>
        /// <returns>The value of <see cref="P:SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T ResetMapping()
        {
            _mappingParser = null;
            return Me;
        }

        /// <summary>
        /// Loads the data from a resource whose templates can be accessed using <see cref="Include"/>.
        /// </summary>
        /// <param name="resource">The resource to load that gives access to a template</param>
        /// <param name="properties">The optional properties to use while loading data from the resource</param>
        /// <returns>The value of <see cref="P:SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T LoadMapping(Uri resource, Properties properties = null)
        {
            var handler = Settings.Instance.GetSchemeHandler(resource, true);
            using (var input = handler.Open(resource))
                MappingParser.LoadConfiguration(input, properties);
            return Me;
        }

        /// <summary>
        /// Includes the data from a given template
        /// </summary>
        /// <param name="templateName">The name of the template to apply to this builder.</param>
        /// <param name="offset">the value to offset configured positions by</param>
        /// <returns>The value of <see cref="P:SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T Include(string templateName, int offset = 0)
        {
            MappingParser.IncludeTemplate(Config, templateName, offset);
            return Me;
        }
    }
}
