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

        public XmlParserBuilder SuppressHeader()
        {
            _parser.SuppressHeader = true;
            return this;
        }

        public XmlParserBuilder HeaderVersion(Version version)
        {
            _parser.Version = version;
            return this;
        }

        public XmlParserBuilder HeaderEncoding(string encoding)
        {
            _parser.Encoding = encoding;
            return this;
        }

        public XmlParserBuilder HeaderEncoding(Encoding encoding)
        {
            _parser.Encoding = encoding.WebName;
            return this;
        }

        public XmlParserBuilder AddNamespace(string prefix, string uri)
        {
            _parser.AddNamespace(prefix, uri);
            return this;
        }

        public XmlParserBuilder Indent()
        {
            return Indent(2);
        }

        public XmlParserBuilder Indent(int amount)
        {
            _parser.Indentation = amount;
            return this;
        }

        public XmlParserBuilder LineSeparator(string sep)
        {
            _parser.LineSeparator = sep;
            return this;
        }

        public BeanConfig<IRecordParserFactory> Build()
        {
            var config = new BeanConfig<IRecordParserFactory>(() => _parser);
            return config;
        }
    }
}
