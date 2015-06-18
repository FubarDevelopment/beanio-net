using System.Collections.Generic;
using System.Xml.Linq;

using BeanIO.Internal.Parser.Format.Xml.Annotations;
using BeanIO.Internal.Util;
using BeanIO.Stream;

namespace BeanIO.Internal.Parser.Format.Xml
{
    internal class XmlMarshallingContext : MarshallingContext
    {
        private readonly Stack<IXmlNode> _groupStack;

        private XDocument _document;

        private XContainer _parent;

        private int _ungroup;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlMarshallingContext"/> class.
        /// </summary>
        /// <param name="groupDepth">the maximum depth of a group in the parser tree</param>
        /// <param name="nameConversionMode">the name conversion mode</param>
        public XmlMarshallingContext(int groupDepth, ElementNameConversionMode nameConversionMode)
        {
            _groupStack = new Stack<IXmlNode>(groupDepth);
            NameConversionMode = nameConversionMode;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a stream is being marshalled, versus a single document.
        /// </summary>
        public bool IsStreaming { get; set; }

        /// <summary>
        /// Gets the element name conversion mode
        /// </summary>
        public ElementNameConversionMode NameConversionMode { get; private set; }

        /// <summary>
        /// Gets the record object to pass to the <see cref="IRecordWriter"/>
        /// when <see cref="MarshallingContext.WriteRecord"/> is called.
        /// </summary>
        public object RecordObject
        {
            get { return Document; }
        }

        /// <summary>
        /// Gets the document being marshalled
        /// </summary>
        public XDocument Document
        {
            get
            {
                return _document;
            }
            private set
            {
                _document = value;
                Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the parent node to append in the document being marshalled.
        /// </summary>
        public XContainer Parent
        {
            get
            {
                if (_parent == null)
                {
                    _document = new XDocument();
                    _parent = _document;

                    while (_groupStack.Count != 0)
                    {
                        var xml = _groupStack.Pop();

                        var element = new XElement(xml.ToXName(false).ToConvertedName(NameConversionMode));
                        if (!string.IsNullOrEmpty(xml.Prefix))
                            element.SetAttributeValue(XNamespace.Xmlns + xml.Prefix, xml.Namespace);

                        element = XElement.Parse(element.ToString());
                        element.AddAnnotation(new IsGroupElementAnnotation(true));
                        if (!xml.IsNamespaceAware)
                        {
                            element.AddAnnotation(new NamespaceModeAnnotation(NamespaceHandlingMode.IgnoreNamespace));
                        }
                        else if (string.Equals(xml.Prefix, string.Empty))
                        {
                            element.AddAnnotation(new NamespaceModeAnnotation(NamespaceHandlingMode.DefaultNamespace));
                        }

                        _parent.Add(element);

                        _parent = element;
                    }
                }
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }

        /// <summary>
        /// Clear is invoked after each bean object (record or group) is marshalled
        /// </summary>
        public override void Clear()
        {
            Document = null;
        }

        /// <summary>
        /// Writes the current record object to the record writer.
        /// </summary>
        /// <remarks>
        /// This method uses the <see cref="MarshallingContext.ToRecordObject"/>.
        /// </remarks>
        public override void WriteRecord()
        {
            base.Clear();

            for (var i = 0; i != _ungroup; ++i)
                RecordWriter.Write(null);
            _ungroup = 0;

            base.WriteRecord();
        }

        /// <summary>
        /// Adds a group to be marshalled when the next record is written to
        /// the output stream.
        /// </summary>
        /// <param name="node">the group element to add</param>
        public virtual void OpenGroup(IXmlNode node)
        {
            _groupStack.Push(node);
        }

        /// <summary>
        /// Indicates a group element should be closed before marshalling the next record.
        /// </summary>
        /// <param name="node">the <see cref="IXmlNode"/> to close</param>
        public virtual void CloseGroup(IXmlNode node)
        {
            ++_ungroup;
        }

        /// <summary>
        /// Converts a record object to a <see cref="XDocument"/>.
        /// </summary>
        /// <param name="record">the record object to convert</param>
        /// <returns>the <see cref="XDocument"/> result, or null if not supported</returns>
        public override XDocument ToXDocument(object record)
        {
            return (XDocument)record;
        }

        /// <summary>
        /// Creates the record object to pass to the <see cref="IRecordWriter"/>
        /// when <see cref="MarshallingContext.WriteRecord"/> is called.
        /// </summary>
        /// <returns>
        /// The newly created record object.
        /// </returns>
        protected override object ToRecordObject()
        {
            return Document;
        }
    }
}
