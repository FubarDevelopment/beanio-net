using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using BeanIO.Internal.Parser.Format.Xml.Annotations;

namespace BeanIO.Stream.Xml
{
    /// <summary>
    /// A <see cref="XmlReader"/> is used to read records from a XML input stream
    /// </summary>
    /// <remarks>
    /// <para>Each XML record read from the input stream is parsed into a Document Object Model (DOM).
    /// A <see cref="XmlReader"/> is configured using a base DOM object to define the group
    /// structure of the XML.  When a XML element is read from the input stream that
    /// is not found in the base document, the element and its children are appended
    /// to the base document to form the <i>record</i>.  The base document object model
    /// will be modified as the input stream is read and should therefore not be
    /// shared across multiple streams.</para>
    /// <para>A <see cref="XmlReader"/> makes use of the DOM user data feature to pass additional
    /// information to and from the parser.  The <see cref="GroupCountAnnotation"/> is an <see cref="int"/>
    /// value added to elements in the base document to indicate the number of times an
    /// element was read from the input stream.  And the <see cref="IsNamespaceIgnoredAnnotation"/> is a
    /// <see cref="bool"/> value set on elements in the base document where the XML namespace
    /// should not be used to match nodes read from the input stream.</para>
    /// </remarks>
    public class XmlReader : IRecordReader
    {
        /// <summary>
        /// the input stream to read from
        /// </summary>
        private readonly System.Xml.XmlReader _in;

        /// <summary>
        /// the base document used to define the group structure of the XML read from the input stream
        /// </summary>
        private readonly XDocument _document;

        /// <summary>
        /// set to true if the base document was null during construction and the XML input stream
        /// will be fully read
        /// </summary>
        private readonly bool _readFully;

        /// <summary>
        /// the parent node is the record node's parent in the base document
        /// </summary>
        private readonly XContainer _parentNode;

        /// <summary>
        /// the "root" element of the last record read
        /// </summary>
        private XElement _recordNode;

        private int _recordLineNumber = -1;

        private bool _eof;

        /// <summary>
        /// Gets a single record from this input stream.
        /// </summary>
        /// <remarks>The type of object returned depends on the format of the stream.</remarks>
        /// <returns>
        /// The record value, or null if the end of the stream was reached.
        /// </returns>
        public int RecordLineNumber { get; private set; }

        /// <summary>
        /// Gets the unparsed record text of the last record read.
        /// </summary>
        /// <returns>
        /// The unparsed text of the last record read
        /// </returns>
        public string RecordText { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlReader"/> class.
        /// </summary>
        /// <param name="reader">the input stream to read from</param>
        public XmlReader(TextReader reader)
            : this(reader, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlReader"/> class.
        /// </summary>
        /// <param name="reader">the input stream to read from</param>
        /// <param name="baseDocument">the base document object model (DOM) that defines the
        /// group structure of the XML. May be <code>null</code> if fully reading
        /// the XML document.</param>
        public XmlReader(TextReader reader, XDocument baseDocument)
        {
            _in = System.Xml.XmlReader.Create(reader, new XmlReaderSettings()
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = false,
                });
            _document = baseDocument ?? new XDocument();
            if (_document.Root == null)
            {
                _readFully = true;
                _parentNode = _document;
            }
            else
            {
                _readFully = false;
                _parentNode = null;
            }
        }

        /// <summary>
        /// Reads a single record from this input stream.
        /// </summary>
        /// <returns>
        /// The type of object returned depends on the format of the stream.
        /// </returns>
        /// <returns>The record value, or null if the end of the stream was reached.</returns>
        public object Read()
        {
            if (_eof)
                return null;

            try
            {
                if (_parentNode != null)
                {
                    if (_recordNode != null)
                    {
                        _recordNode.Remove();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new RecordIOException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Closes this input stream.
        /// </summary>
        public void Close()
        {
            throw new System.NotImplementedException();
        }
    }
}