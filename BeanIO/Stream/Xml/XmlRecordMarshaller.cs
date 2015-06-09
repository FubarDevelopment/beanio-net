﻿using System;
using System.IO;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace BeanIO.Stream.Xml
{
    public class XmlRecordMarshaller : IRecordMarshaller
    {
        private readonly XmlParserConfiguration _config;

        private readonly XmlWriterSettings _writerSettings;

        public XmlRecordMarshaller()
            : this(new XmlParserConfiguration())
        {
        }

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
                var encoding = _config.Encoding != null ? _config.Encoding.WebName : null;
                document.Declaration = new XDeclaration(version, encoding, null);
            }

            var output = new StringWriter();
            using (var xmlWriter = XmlWriter.Create(output, _writerSettings))
                document.WriteTo(xmlWriter);

            return output.ToString();
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
                };
            return settings;
        }
    }
}