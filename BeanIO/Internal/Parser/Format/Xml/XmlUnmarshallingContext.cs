using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser.Format.Xml
{
    public class XmlUnmarshallingContext : UnmarshallingContext
    {
        /// <summary>
        /// This stack of elements is used to store the last XML node parsed for a field or bean collection.
        /// </summary>
        private readonly Stack<XElement> _elementStack = new Stack<XElement>();

        /// <summary>
        /// Store previously matched groups for parsing subsequent records in a record group
        /// </summary>
        private readonly Stack<IXmlNode> _groupStack;

        /// <summary>
        /// The DOM to parse
        /// </summary>
        private XDocument _document;

        /// <summary>
        /// The last parsed node in the document, which is the parent node of the next field/bean to parse
        /// </summary>
        private XElement _position;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlUnmarshallingContext"/> class.
        /// </summary>
        /// <param name="groupDepth">the maximum depth of an element mapped to a <see cref="Group"/> in the DOM</param>
        public XmlUnmarshallingContext(int groupDepth)
        {
            _groupStack = new Stack<IXmlNode>(groupDepth);
        }

        /// <summary>
        /// Gets the XML document object model (DOM) for the current record.
        /// </summary>
        public XDocument Document
        {
            get { return _document; }
        }

        /// <summary>
        /// Gets the current unmarshalled position in the DOM tree, or null
        /// if a node has not been matched yet.
        /// </summary>
        public XElement Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Gets or sets the last parsed DOM element for a field or bean collection.
        /// </summary>
        public XElement PreviousElement
        {
            get
            {
                return _elementStack.Peek();
            }
            set
            {
                _elementStack.Pop();
                _elementStack.Push(value);
            }
        }

        /// <summary>
        /// Sets the value of the record returned from the <see cref="IRecordReader"/>
        /// </summary>
        /// <param name="value">the record value read by a <see cref="IRecordReader"/></param>
        public override void SetRecordValue(object value)
        {
            var node = (XNode)value;
            switch (node.NodeType)
            {
                case XmlNodeType.Document:
                    _document = (XDocument)value;
                    _position = null;
                    break;
                case XmlNodeType.Element:
                    _document = node.Document;
                    _position = (XElement)node;
                    break;
                default:
                    _document = node.Document;
                    _position = null;
                    break;
            }
        }

        /// <summary>
        /// Pushes an <see cref="IIteration"/> onto a stack for adjusting
        /// field positions and indices.
        /// </summary>
        /// <param name="iteration">the <see cref="IIteration"/> to push</param>
        public override void PushIteration(IIteration iteration)
        {
            base.PushIteration(iteration);
            _elementStack.Push(null);
        }

        /// <summary>
        /// Pops the last <see cref="IIteration"/> pushed onto the stack.
        /// </summary>
        /// <returns>the top most <see cref="IIteration"/></returns>
        public override IIteration PopIteration()
        {
            _elementStack.Pop();
            return base.PopIteration();
        }

        /// <summary>
        /// Updates <see cref="Position"/> by finding a child of the current position
        /// that matches a given node.
        /// </summary>
        /// <remarks>
        /// If <tt>isGroup</tt> is true, the node is indexed by its depth so that
        /// calls to this method for subsequent records in the same group can
        /// update <see cref="Position"/> according to the depth of the record.
        /// </remarks>
        /// <param name="node">the <see cref="IXmlNode"/> to match</param>
        /// <param name="depth">e depth of the node in the DOM tree</param>
        /// <param name="isGroup">whether the node is mapped to a <see cref="Group"/></param>
        /// <returns>the matched node or null if not matched</returns>
        public virtual XElement PushPosition(IXmlNode node, int depth, bool isGroup)
        {
            
        }
    }
}
