// <copyright file="XmlAttributeField.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Format.Xml
{
    /// <summary>
    /// A <see cref="IFieldFormat"/> for a field in an XML formatted stream parsed as
    /// an attribute of its parent.
    /// </summary>
    internal class XmlAttributeField : XmlFieldFormat
    {
        private string? _localName;

        private string? _namespace;

        private string? _prefix;

        private bool _namespaceAware;

        /// <summary>
        /// Gets the XML node type.
        /// </summary>
        public override XmlNodeType Type => XmlNodeType.Attribute;

        /// <summary>
        /// Gets the XML local name for this node.
        /// </summary>
        public override string? LocalName => _localName;

        /// <summary>
        /// Gets the namespace of this node.  If there is no namespace for this
        /// node, or this node is not namespace aware, <see langword="null" /> is returned.
        /// </summary>
        public override string? Namespace => _namespace;

        /// <summary>
        /// Gets a value indicating whether a namespace was configured for this node,
        /// and is therefore used to unmarshal and marshal the node.
        /// </summary>
        public override bool IsNamespaceAware => _namespaceAware;

        /// <summary>
        /// Gets the namespace prefix for marshaling this node, or <see langword="null" />
        /// if the namespace should override the default namespace.
        /// </summary>
        public override string? Prefix => _prefix;

        /// <summary>
        /// Gets a value indicating whether this field is nillable.
        /// </summary>
        public override bool IsNillable => false;

        /// <summary>
        /// Gets a value indicating whether this node may repeat in the context of its immediate parent.
        /// </summary>
        public override bool IsRepeating => false;

        /// <summary>
        /// Inserts a field into the record during marshalling.
        /// </summary>
        /// <param name="context">the <see cref="XmlMarshallingContext"/> holding the record.</param>
        /// <param name="fieldText">the field text to insert.</param>
        public override void InsertText(XmlMarshallingContext context, string? fieldText)
        {
            var parent = (XElement?)context.Parent ?? throw new InvalidOperationException("No parent element");

            // format the field text (a null field value may not return null if a custom type handler was configured)
            var text = fieldText;

            // nothing to marshal if minOccurs is 0
            if (text == null && IsLazy)
                return;

            if (parent.NodeType == XmlNodeType.Element)
            {
                if (text == null)
                    text = string.Empty;

                var att = new XAttribute(this.ToXName(true).ToConvertedName(context.NameConversionMode), text);
                if (!string.IsNullOrEmpty(Prefix))
                    parent.SetAttributeValue(XNamespace.Xmlns + Prefix, Namespace);
                parent.Add(att);
            }
        }

        /// <summary>
        /// Sets the attribute name.
        /// </summary>
        /// <param name="localName">the attribute name.</param>
        public void SetLocalName(string? localName)
        {
            _localName = localName;
        }

        /// <summary>
        /// Sets the namespace of the attribute (typically null).
        /// </summary>
        /// <param name="ns">the attribute namespace.</param>
        public void SetNamespace(string? ns)
        {
            _namespace = ns;
        }

        /// <summary>
        /// Sets the prefix to use for this attribute's namespace.
        /// </summary>
        /// <param name="prefix"> the namespace prefix.</param>
        public void SetPrefix(string? prefix)
        {
            _prefix = prefix;
        }

        /// <summary>
        /// Sets whether this attribute uses a namespace.
        /// </summary>
        /// <param name="namespaceAware">true if this attribute uses a namespace, false otherwise.</param>
        public void SetNamespaceAware(bool namespaceAware)
        {
            _namespaceAware = namespaceAware;
        }

        /// <summary>
        /// Called by <see cref="XmlFieldFormat.ToString"/> to append attributes of this field.
        /// </summary>
        /// <param name="s">the <see cref="StringBuilder"/> to write to.</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            s.AppendFormat(", localName={0}", LocalName);
            if (Prefix != null)
                s.AppendFormat(", prefix={0}", Prefix);
            if (Namespace != null)
                s.AppendFormat(", xmlns={0}", Namespace);
        }

        /// <summary>
        /// Extracts a field from a record during unmarshalling.
        /// </summary>
        /// <param name="context">the <see cref="XmlUnmarshallingContext"/> holding the record.</param>
        /// <returns>the extracted field text.</returns>
        protected override string? ExtractText(XmlUnmarshallingContext context)
        {
            var parent = context.Position;
            return parent?.GetAttribute(this, this.GetNameVariants());
        }
    }
}
