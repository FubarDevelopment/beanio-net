// <copyright file="XmlRecordParserFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.IO;
using System.Xml.Linq;

namespace BeanIO.Stream.Xml
{
    /// <summary>
    /// Default <see cref="IRecordParserFactory"/> for the XML stream format.
    /// </summary>
    public class XmlRecordParserFactory : XmlParserConfiguration, IRecordParserFactory, IXmlStreamConfigurationAware
    {
        private IXmlStreamConfiguration _source;

        /// <summary>
        /// Initializes the factory.
        /// </summary>
        /// <remarks>
        /// This method is called when a mapping file is loaded after
        /// all parser properties have been set, and is therefore ideally used to preemptively
        /// validate parser configuration settings.
        /// </remarks>
        public void Init()
        {
        }

        /// <summary>
        /// Creates a parser for reading records from an input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from</param>
        /// <returns>The created <see cref="IRecordReader"/></returns>
        public IRecordReader CreateReader(TextReader reader)
        {
            XDocument doc = _source?.CreateDocument();
            return new XmlReader(reader, doc);
        }

        /// <summary>
        /// Creates a parser for writing records to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to</param>
        /// <returns>The new <see cref="IRecordWriter"/></returns>
        public IRecordWriter CreateWriter(TextWriter writer)
        {
            return new XmlWriter(writer, this);
        }

        /// <summary>
        /// Creates a parser for marshalling records.
        /// </summary>
        /// <returns>The created <see cref="IRecordMarshaller"/></returns>
        public IRecordMarshaller CreateMarshaller()
        {
            return new XmlRecordMarshaller(this);
        }

        /// <summary>
        /// Creates a parser for unmarshalling records.
        /// </summary>
        /// <returns>The created <see cref="IRecordUnmarshaller"/></returns>
        public IRecordUnmarshaller CreateUnmarshaller()
        {
            return new XmlRecordUnmarshaller();
        }

        /// <summary>
        /// This method is invoked by a XML stream definition when a <see cref="IRecordReader"/>
        /// implementation is registered.
        /// </summary>
        /// <param name="configuration">the XML stream configuration</param>
        public void Configure(IXmlStreamConfiguration configuration)
        {
            _source = configuration;
        }
    }
}
