using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using BeanIO.Internal.Parser.Format.Xml.Annotations;
using BeanIO.Internal.Util;

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
    /// element was read from the input stream.  And the <see cref="NamespaceModeAnnotation"/> is a
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
        private XContainer _parentNode;

        /// <summary>
        /// the "root" element of the last record read
        /// </summary>
        private XElement _recordNode;

        private bool _eof;

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
            RecordLineNumber = -1;
            _in = System.Xml.XmlReader.Create(
                reader,
                new XmlReaderSettings()
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true,
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
        public string RecordText
        {
            get { return _document.ToString(); }
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
                        _recordNode = null;
                    }
                }

                return ReadRecord() ? _document : null;
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
            _in.Dispose();
        }

        /// <summary>
        /// Appends the next record read from the XML stream reader to the base document object model
        /// </summary>
        /// <remarks>
        /// TODO: Isn't there a better way?
        /// </remarks>
        /// <returns><code>true</code> if a record was found, or <tt>false</tt> if the end of the
        /// stream was reached</returns>
        private bool ReadRecord()
        {
            // the record position stores the number of elements deep in the record, or -1 if a
            // record has not been found yet
            int recordPosition = _readFully ? 0 : -1;

            // the parent element to the node we are reading
            var node = _parentNode;

            while (_in.Read())
            {
                bool closeElement = false;
                switch (_in.NodeType)
                {
                    case XmlNodeType.Element:
                        closeElement = _in.IsEmptyElement;
                        if (recordPosition < 0)
                        {
                            // handle the root element of the document
                            if (node == null)
                            {
                                node = _document.Root;
                                if (IsNode((XElement)node, _in.NamespaceURI, _in.LocalName))
                                {
                                    node.SetAnnotation(new GroupCountAnnotation(1));
                                    continue;
                                }
                            }
                            else
                            {
                                // try to find a child in the base document that matches the element we just read
                                var baseElement = FindChild((XElement)node, _in.NamespaceURI, _in.LocalName);
                                if (baseElement != null)
                                {
                                    // if found, increment its counter and continue
                                    var groupCount = baseElement.Annotation<GroupCountAnnotation>();
                                    var count = groupCount != null ? groupCount.Count : 0;
                                    baseElement.SetAnnotation(new GroupCountAnnotation(count + 1));
                                    node = baseElement;
                                    continue;
                                }
                            }

                            // if we find an element not included in the base document, this is the beginning of our record
                            RecordLineNumber = ((IXmlLineInfo)_in).LineNumber;
                            _parentNode = node;
                        }

                        // create and append the new element to our Document
                        var e = new XElement(XNamespace.Get(_in.NamespaceURI) + _in.LocalName);
                        for (int i = 0, j = _in.AttributeCount; i < j; i++)
                        {
                            _in.MoveToAttribute(i);
                            if (_in.NamespaceURI == XNamespace.Xmlns.NamespaceName && _in.LocalName == "xmlns")
                                continue;
                            var attrName = XNamespace.Get(_in.NamespaceURI) + _in.LocalName;
                            e.SetAttributeValue(attrName, _in.GetAttribute(i));
                        }
                        node.Add(e);
                        node = e;

                        // set the record node if this is the "root" element of the record
                        if (_recordNode == null)
                        {
                            _recordNode = (XElement)node;
                        }

                        ++recordPosition;
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        if (recordPosition >= 0)
                            node.Add(new XText(_in.Value));
                        break;
                    case XmlNodeType.EndElement:
                        closeElement = true;
                        break;
                }

                if (closeElement)
                {
                    var parent = node.Parent;
                    if (parent != null && parent.NodeType == XmlNodeType.Element)
                    {
                        node = parent;
                    }
                    else
                    {
                        node = null;
                    }

                    if (recordPosition < 0)
                        continue;

                    // if the record position reaches 0, the record is complete
                    if (recordPosition-- == 0)
                    {
                        return true;
                    }
                }
            }

            _eof = true;
            return _readFully;
        }

        /// <summary>
        /// Searches a DOM element for a child element matching the given XML namespace and local name.
        /// </summary>
        /// <param name="parent">the parent DOM element</param>
        /// <param name="ns">the XML namespace to match</param>
        /// <param name="name">the XML local name to match</param>
        /// <returns>the matched child element, or <code>null</code> if not found</returns>
        private XElement FindChild(XElement parent, string ns, string name)
        {
            return parent.Elements().FirstOrDefault(x => IsNode(x, ns, name));
        }

        /// <summary>
        /// Returns whether a XML node matches a given namespace and local name
        /// </summary>
        /// <param name="node">the Node to test</param>
        /// <param name="ns">the namespace to match</param>
        /// <param name="name">the local name to match</param>
        /// <returns><code>true</code> if the Node matches the given XML namespace and local name</returns>
        private bool IsNode(XElement node, string ns, string name)
        {
            var handlingModeAttr = node.Annotation<NamespaceModeAnnotation>();
            var handlingMode = handlingModeAttr == null ? NamespaceHandlingMode.UseNamespace : handlingModeAttr.HandlingMode;

            return NamespaceAwareElementComparer.Compare(
                handlingMode,
                node.Name,
                NamespaceHandlingMode.UseNamespace,
                XNamespace.Get(ns) + name) == 0;
        }
    }
}
