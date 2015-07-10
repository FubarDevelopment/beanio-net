using System;
using System.Text;

using BeanIO.Internal.Config;
using BeanIO.Stream;
using BeanIO.Stream.Xml;

namespace BeanIO.Builder
{
    /// <summary>
    /// Builder for XML parsers
    /// </summary>
    public class XmlParserBuilder : IParserBuilder
    {
        private readonly XmlRecordParserFactory _parser = new XmlRecordParserFactory();

        /// <summary>
        /// Suppress the XML header?
        /// </summary>
        /// <returns>the <see cref="XmlParserBuilder"/></returns>
        public XmlParserBuilder SuppressHeader()
        {
            _parser.SuppressHeader = true;
            return this;
        }

        /// <summary>
        /// Sets the XML version
        /// </summary>
        /// <param name="version">the XML version to set</param>
        /// <returns>the <see cref="XmlParserBuilder"/></returns>
        public XmlParserBuilder HeaderVersion(Version version)
        {
            _parser.Version = version;
            return this;
        }

        /// <summary>
        /// Sets the XML encoding
        /// </summary>
        /// <param name="encoding">The XML encoding to set</param>
        /// <returns>the <see cref="XmlParserBuilder"/></returns>
        public XmlParserBuilder HeaderEncoding(string encoding)
        {
            _parser.Encoding = encoding;
            return this;
        }

        /// <summary>
        /// Sets the XML encoding
        /// </summary>
        /// <param name="encoding">The XML encoding to set</param>
        /// <returns>the <see cref="XmlParserBuilder"/></returns>
        public XmlParserBuilder HeaderEncoding(Encoding encoding)
        {
            _parser.Encoding = encoding.WebName;
            return this;
        }

        /// <summary>
        /// Sets a known prefix for an XML namespace
        /// </summary>
        /// <param name="prefix">The namespace prefix</param>
        /// <param name="uri">The XML namespace</param>
        /// <returns>the <see cref="XmlParserBuilder"/></returns>
        public XmlParserBuilder AddNamespace(string prefix, string uri)
        {
            _parser.AddNamespace(prefix, uri);
            return this;
        }

        /// <summary>
        /// Sets the XML indention level to 2.
        /// </summary>
        /// <returns>the <see cref="XmlParserBuilder"/></returns>
        public XmlParserBuilder Indent()
        {
            return Indent(2);
        }

        /// <summary>
        /// Sets the XML indention level.
        /// </summary>
        /// <param name="amount">The number of characters to indent the XML</param>
        /// <returns>the <see cref="XmlParserBuilder"/></returns>
        public XmlParserBuilder Indent(int amount)
        {
            _parser.Indentation = amount;
            return this;
        }

        /// <summary>
        /// Sets the line separator to be used when indenting the XML file
        /// </summary>
        /// <param name="sep">the line separator to use</param>
        /// <returns>the <see cref="XmlParserBuilder"/></returns>
        public XmlParserBuilder LineSeparator(string sep)
        {
            _parser.LineSeparator = sep;
            return this;
        }

        /// <summary>
        /// Builds the configuration about the record parser factory.
        /// </summary>
        /// <returns>The configuration for the record parser factory.</returns>
        public BeanConfig<IRecordParserFactory> Build()
        {
            var config = new BeanConfig<IRecordParserFactory>(() => _parser);
            return config;
        }
    }
}
