// <copyright file="XmlTextField.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Xml;
using System.Xml.Linq;

namespace BeanIO.Internal.Parser.Format.Xml
{
    internal class XmlTextField : XmlFieldFormat
    {
        /// <summary>
        /// Gets the XML node type
        /// </summary>
        public override XmlNodeType Type => XmlNodeType.Text;

        /// <summary>
        /// Gets the XML local name for this node.
        /// </summary>
        public override string LocalName => null;

        /// <summary>
        /// Gets the namespace of this node.  If there is no namespace for this
        /// node, or this node is not namespace aware, <code>null</code> is returned.
        /// </summary>
        public override string Namespace => null;

        /// <summary>
        /// Gets a value indicating whether a namespace was configured for this node,
        /// and is therefore used to unmarshal and marshal the node.
        /// </summary>
        public override bool IsNamespaceAware => false;

        /// <summary>
        /// Gets the namespace prefix for marshaling this node, or <code>null</code>
        /// if the namespace should override the default namespace.
        /// </summary>
        public override string Prefix => null;

        /// <summary>
        /// Gets a value indicating whether this field is nillable
        /// </summary>
        public override bool IsNillable => false;

        /// <summary>
        /// Gets a value indicating whether this node may repeat in the context of its immediate parent.
        /// </summary>
        public override bool IsRepeating => false;

        /// <summary>
        /// Inserts a field into the record during marshalling
        /// </summary>
        /// <param name="context">the <see cref="XmlMarshallingContext"/> holding the record</param>
        /// <param name="text">the field text to insert</param>
        public override void InsertText(XmlMarshallingContext context, string text)
        {
            if (text == null)
                return;

            var parent = (XElement)context.Parent;
            if (parent != null)
                parent.Value = text;
        }

        /// <summary>
        /// Extracts a field from a record during unmarshalling
        /// </summary>
        /// <param name="context">the <see cref="XmlUnmarshallingContext"/> holding the record</param>
        /// <returns>the extracted field text</returns>
        protected override string ExtractText(XmlUnmarshallingContext context)
        {
            var parent = context.Position;
            if (parent == null)
                return null;
            var fieldText = parent.GetText() ?? string.Empty;
            return fieldText;
        }
    }
}
