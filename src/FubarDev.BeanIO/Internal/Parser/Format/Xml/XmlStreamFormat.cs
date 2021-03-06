// <copyright file="XmlStreamFormat.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Xml.Linq;

using BeanIO.Stream;
using BeanIO.Stream.Xml;

namespace BeanIO.Internal.Parser.Format.Xml
{
    internal class XmlStreamFormat : StreamFormatSupport
    {
        /// <summary>
        /// Gets or sets the root node of the parser tree
        /// </summary>
        public ISelector Layout { get; set; }

        /// <summary>
        /// Gets or sets the maximum depth of a group component in the parser tree
        /// </summary>
        public int GroupDepth { get; set; }

        /// <summary>
        /// Gets or sets the element name conversion mode
        /// </summary>
        public ElementNameConversionMode NameConversionMode { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IRecordParserFactory"/> used by this stream
        /// </summary>
        public override IRecordParserFactory RecordParserFactory
        {
            get
            {
                return base.RecordParserFactory;
            }
            set
            {
                var configAware = value as IXmlStreamConfigurationAware;
                configAware?.Configure(new RecordParserXmlStreamConfiguration(this));
                base.RecordParserFactory = value;
            }
        }

        /// <summary>
        /// Creates a new unmarshalling context
        /// </summary>
        /// <returns>the new <see cref="UnmarshallingContext"/></returns>
        public override UnmarshallingContext CreateUnmarshallingContext()
        {
            return new XmlUnmarshallingContext(GroupDepth);
        }

        /// <summary>
        /// Creates a new marshalling context
        /// </summary>
        /// <param name="streaming">true if marshalling to a stream</param>
        /// <returns>the new <see cref="MarshallingContext"/></returns>
        public override MarshallingContext CreateMarshallingContext(bool streaming)
        {
            var ctx = new XmlMarshallingContext(GroupDepth, NameConversionMode)
                {
                    IsStreaming = streaming
                };
            return ctx;
        }

        /// <summary>
        /// Creates a DOM made up of any group nodes in the parser tree.
        /// </summary>
        /// <param name="layout">the <see cref="XmlSelectorWrapper"/></param>
        /// <param name="nameConversionMode">the element and attribute name conversion mode</param>
        /// <returns>the new <see cref="XDocument"/> made up of group nodes</returns>
        protected virtual XDocument CreateBaseDocument(ISelector layout, ElementNameConversionMode nameConversionMode)
        {
            var wrapper = layout as XmlSelectorWrapper;
            if (wrapper == null)
                return new XDocument();
            return wrapper.CreateBaseDocument(nameConversionMode);
        }

        private class RecordParserXmlStreamConfiguration : IXmlStreamConfiguration
        {
            private readonly XmlStreamFormat _format;

            public RecordParserXmlStreamConfiguration(XmlStreamFormat format)
            {
                _format = format;
            }

            public XDocument CreateDocument()
                => _format.CreateBaseDocument(_format.Layout, _format.NameConversionMode);
        }
    }
}
