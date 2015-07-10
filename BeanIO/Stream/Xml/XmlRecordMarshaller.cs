using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BeanIO.Stream.Xml
{
    /// <summary>
    /// A <see cref="IRecordMarshaller"/> implementation for XML formatted records.
    /// </summary>
    public class XmlRecordMarshaller : IRecordMarshaller
    {
        private readonly XmlParserConfiguration _config;

        private readonly XmlWriterSettings _writerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRecordMarshaller"/> class.
        /// </summary>
        public XmlRecordMarshaller()
            : this(new XmlParserConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlRecordMarshaller"/> class.
        /// </summary>
        /// <param name="config">the <see cref="XmlRecordMarshaller"/> configuration</param>
        public XmlRecordMarshaller(XmlParserConfiguration config)
        {
            _config = config ?? new XmlParserConfiguration();
            _writerSettings = Init();
        }

        /// <summary>
        /// Marshals a single record object to a <code>String</code>.
        /// </summary>
        /// <param name="record">Record the record object to marshal</param>
        /// <returns>The marshalled record text</returns>
        public string Marshal(object record)
        {
            try
            {
                return Marshal((XDocument)record);
            }
            catch (Exception ex)
            {
                throw new RecordIOException(string.Format("Failed to marshal XML record: {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Marshals a <see cref="XDocument"/>
        /// </summary>
        /// <param name="document">the <see cref="XDocument"/> to marshal</param>
        /// <returns>the marshalled record text</returns>
        protected virtual string Marshal(XDocument document)
        {
            if (!_config.SuppressHeader)
            {
                var version = _config.Version != null ? _config.Version.ToString() : null;
                document.Declaration = new XDeclaration(version, _config.Encoding, null);
            }

            var outputEncoding = _writerSettings.Encoding;
            var temp = new MemoryStream();
            var output = new StreamWriter(temp, outputEncoding);
            using (var xmlWriter = System.Xml.XmlWriter.Create(output, _writerSettings))
                document.WriteTo(xmlWriter);

            var data = temp.ToArray();
            var result = outputEncoding.GetString(data, 0, data.Length);

            return result;
        }

        /// <summary>
        /// Initializes this writer after the configuration has been set
        /// </summary>
        /// <returns>the new <see cref="XmlWriterSettings"/></returns>
        private XmlWriterSettings Init()
        {
            var settings = new XmlWriterSettings()
                {
                    NewLineChars = _config.LineSeparator ?? Environment.NewLine,
                    Indent = _config.IsIndentionEnabled,
                    IndentChars = new string(' ', _config.Indentation.GetValueOrDefault()),
                    NamespaceHandling = NamespaceHandling.OmitDuplicates,
                    OmitXmlDeclaration = _config.SuppressHeader,
                    Encoding = _config.GetEncoding() ?? new UTF8Encoding(false),
                };
            return settings;
        }
    }
}
