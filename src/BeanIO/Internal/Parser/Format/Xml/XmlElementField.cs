// <copyright file="XmlElementField.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using BeanIO.Internal.Parser.Format.Xml.Annotations;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Format.Xml
{
    /// <summary>
    /// A <see cref="IFieldFormat"/> for a field in an XML formatted stream parsed as
    /// an element of its parent.
    /// </summary>
    internal class XmlElementField : XmlFieldFormat
    {
        private string _localName;

        private string _namespace;

        private string _prefix;

        private bool _namespaceAware;

        private bool _repeating;

        private bool _nillable;

        /// <summary>
        /// Gets the XML node type
        /// </summary>
        public override XmlNodeType Type => XmlNodeType.Element;

        /// <summary>
        /// Gets the XML local name for this node.
        /// </summary>
        public override string LocalName => _localName;

        /// <summary>
        /// Gets the namespace of this node.  If there is no namespace for this
        /// node, or this node is not namespace aware, <code>null</code> is returned.
        /// </summary>
        public override string Namespace => _namespace;

        /// <summary>
        /// Gets a value indicating whether a namespace was configured for this node,
        /// and is therefore used to unmarshal and marshal the node.
        /// </summary>
        public override bool IsNamespaceAware => _namespaceAware;

        /// <summary>
        /// Gets the namespace prefix for marshaling this node, or <code>null</code>
        /// if the namespace should override the default namespace.
        /// </summary>
        public override string Prefix => _prefix;

        /// <summary>
        /// Gets a value indicating whether this field is nillable
        /// </summary>
        public override bool IsNillable => _nillable;

        /// <summary>
        /// Gets a value indicating whether this node may repeat in the context of its immediate parent.
        /// </summary>
        public override bool IsRepeating => _repeating;

        /// <summary>
        /// Inserts a field into the record during marshalling
        /// </summary>
        /// <param name="ctx">the <see cref="XmlMarshallingContext"/> holding the record</param>
        /// <param name="fieldText">the field text to insert</param>
        public override void InsertText(XmlMarshallingContext ctx, string fieldText)
        {
            if (fieldText == null && IsLazy)
                return;
            if (ReferenceEquals(fieldText, Value.Nil))
                fieldText = null;

            var element = new XElement(this.ToXName(true).ToConvertedName(ctx.NameConversionMode));
            var annotations = new List<object>();
            if (!IsNamespaceAware)
            {
                annotations.Add(new NamespaceModeAnnotation(NamespaceHandlingMode.IgnoreNamespace));
            }
            else if (string.IsNullOrEmpty(Namespace))
            {
                annotations.Add(new NamespaceModeAnnotation(NamespaceHandlingMode.DefaultNamespace));
            }
            else if (string.Equals(Prefix, string.Empty))
            {
                annotations.Add(new NamespaceModeAnnotation(NamespaceHandlingMode.NoPrefix));
            }
            else if (Prefix != null)
            {
                element.SetAttributeValue(XNamespace.Xmlns + Prefix, Namespace);
            }

            element = XElement.Parse(element.ToString());
            foreach (var annotation in annotations)
                element.SetAnnotation(annotation);

            if (fieldText == null && IsNillable)
            {
                element.SetNil();
            }
            else if (!string.IsNullOrEmpty(fieldText))
            {
                element.Add(new XText(fieldText));
            }

            var parent = ctx.Parent;
            parent.Add(element);
        }

        /// <summary>
        /// Sets the attribute name.
        /// </summary>
        /// <param name="localName">the attribute name</param>
        public void SetLocalName(string localName)
        {
            _localName = localName;
        }

        /// <summary>
        /// Sets the namespace of the attribute (typically null).
        /// </summary>
        /// <param name="ns">the attribute namespace</param>
        public void SetNamespace(string ns)
        {
            _namespace = ns;
        }

        /// <summary>
        /// Sets the prefix to use for this attribute's namespace.
        /// </summary>
        /// <param name="prefix">the namespace prefix</param>
        public void SetPrefix(string prefix)
        {
            _prefix = prefix;
        }

        /// <summary>
        /// Sets whether this attribute uses a namespace
        /// </summary>
        /// <param name="namespaceAware">true if this attribute uses a namespace, false otherwise</param>
        public void SetNamespaceAware(bool namespaceAware)
        {
            _namespaceAware = namespaceAware;
        }

        /// <summary>
        /// Sets a value indicating whether this element repeats within the context of its parent
        /// </summary>
        /// <param name="repeating">true if repeating, false otherwise</param>
        public void SetRepeating(bool repeating)
        {
            _repeating = repeating;
        }

        /// <summary>
        /// Sets a value indicating whether this element is nillable
        /// </summary>
        /// <param name="nillable">true if nillable, false otherwise</param>
        public void SetNillable(bool nillable)
        {
            _nillable = nillable;
        }

        /// <summary>
        /// Extracts a field from a record during unmarshalling
        /// </summary>
        /// <param name="context">the <see cref="XmlUnmarshallingContext"/> holding the record</param>
        /// <returns>the extracted field text</returns>
        protected override string ExtractText(XmlUnmarshallingContext context)
        {
            var node = context.FindElement(this);
            if (node == null)
                return null;

            // check for nil elements
            if (node.IsNil())
                return Value.Nil;

            var fieldText = node.GetText() ?? string.Empty;
            return fieldText;
        }

        /// <summary>
        /// Called by <see cref="XmlFieldFormat.ToString"/> to append attributes of this field.
        /// </summary>
        /// <param name="s">the string builder to add the parameters to</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            s.AppendFormat(", localName={0}", LocalName);
            if (Prefix != null)
                s.AppendFormat(", prefix={0}", Prefix);
            if (Namespace != null)
                s.AppendFormat(", xmlns={0}", Namespace);
            s.AppendFormat(", {0}", DebugUtil.FormatOption("nillable", IsNillable));
        }
    }
}
