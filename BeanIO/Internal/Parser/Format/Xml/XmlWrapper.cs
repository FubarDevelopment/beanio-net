using System.Text;
using System.Xml;
using System.Xml.Linq;

using BeanIO.Internal.Parser.Format.Xml.Annotations;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Format.Xml
{
    internal class XmlWrapper : DelegatingParser, IXmlNode
    {
        /// <summary>
        /// Gets the XML node type
        /// </summary>
        public XmlNodeType Type
        {
            get { return XmlNodeType.Element; }
        }

        /// <summary>
        /// Gets or sets the XML local name for this node.
        /// </summary>
        public string LocalName { get; set; }

        /// <summary>
        /// Gets or sets the namespace of this node.  If there is no namespace for this
        /// node, or this node is not namespace aware, <code>null</code> is returned.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a namespace was configured for this node,
        /// and is therefore used to unmarshal and marshal the node.
        /// </summary>
        public bool IsNamespaceAware { get; set; }

        /// <summary>
        /// Gets or sets the namespace prefix for marshaling this node, or <code>null</code>
        /// if the namespace should override the default namespace.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this node is nillable.
        /// </summary>
        public bool IsNillable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this node may repeat in the context of its immediate parent.
        /// </summary>
        public bool IsRepeating { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this node is optional.
        /// </summary>
        public bool IsLazy { get; set; }

        /// <summary>
        /// Gets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        public override bool IsOptional
        {
            get { return IsLazy; }
        }

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if matched, false otherwise</returns>
        public override bool Matches(UnmarshallingContext context)
        {
            if (!IsIdentifier)
                return true;

            var ctx = (XmlUnmarshallingContext)context;
            if (ctx.PushPosition(this) == null)
                return false;

            try
            {
                return base.Matches(context);
            }
            finally
            {
                ctx.PopPosition();
            }
        }

        /// <summary>
        /// Unmarshals a record
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise</returns>
        public override bool Unmarshal(UnmarshallingContext context)
        {
            var ctx = (XmlUnmarshallingContext)context;
            if (ctx.PushPosition(this) == null)
                return false;

            try
            {
                // check for nil
                if (ctx.Position.IsNil())
                {
                    if (!IsNillable)
                        context.AddFieldError(Name, null, "nillable");
                }
                else
                {
                    base.Unmarshal(context);
                }
                return true;
            }
            finally
            {
                ctx.PopPosition();
            }
        }

        /// <summary>
        /// Marshals a record
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/></param>
        /// <returns>whether a value was marshalled</returns>
        public override bool Marshal(MarshallingContext context)
        {
            var contentChecked = false;

            if (IsLazy && !IsRepeating)
            {
                if (!HasContent(context))
                    return false;
                contentChecked = true;
            }

            var ctx = (XmlMarshallingContext)context;
            var parent = ctx.Parent;
            var parentElement = ctx.Parent as XElement;

            // create an element for this node
            var ns = Namespace ?? (IsNamespaceAware ? string.Empty : (parentElement == null ? string.Empty : parentElement.Name.NamespaceName));
            var element = new XElement(XNamespace.Get(ns) + LocalName.ToConvertedName(ctx.NameConversionMode));
            if (!IsNamespaceAware)
            {
                element.SetAnnotation(new NamespaceModeAnnotation(NamespaceHandlingMode.IgnoreNamespace));
            }
            else if (string.IsNullOrEmpty(Prefix))
            {
                element.SetAnnotation(new NamespaceModeAnnotation(NamespaceHandlingMode.DefaultNamespace));
            }
            else
            {
                element.SetAttributeValue(XNamespace.Xmlns + Prefix, Namespace);
            }
            element = XElement.Parse(element.ToString());

            // append the new element to its parent
            parent.Add(element);

            // if nillable and there is no descendant with content, mark the element nil
            if (IsNillable && !contentChecked && !HasContent(context))
            {
                element.SetNil();
            }
            else
            {
                // otherwise marshal our descendants
                ctx.Parent = element;
                base.Marshal(context);
                ctx.Parent = parent;
            }

            return true;
        }

        /// <summary>
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            s.AppendFormat(", element={0}", LocalName);
            if (!string.IsNullOrEmpty(Prefix))
                s.AppendFormat(", prefix={0}", Prefix);
            if (Namespace != null)
                s.AppendFormat(", xmlns={0}", IsNamespaceAware ? Namespace : "*");
            s.AppendFormat(", {0}", DebugUtil.FormatOption("lazy", IsLazy))
             .AppendFormat(", {0}", DebugUtil.FormatOption("nillable", IsNillable))
             .AppendFormat(", {0}", DebugUtil.FormatOption("repeating", IsRepeating));
        }
    }
}
