using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using BeanIO.Internal.Parser.Format.Xml.Annotations;
using BeanIO.Internal.Util;
using BeanIO.Stream;

namespace BeanIO.Internal.Parser.Format.Xml
{
    internal class XmlSelectorWrapper : ParserComponent, ISelector, IXmlNode
    {
        /// <summary>
        /// map key used to store the state of the 'addToHierarchy' attribute
        /// </summary>
        private static readonly string WRITTEN_KEY = "written";

        /// <summary>
        /// state attributes
        /// </summary>
        private readonly ParserLocal<bool> _written = new ParserLocal<bool>(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSelectorWrapper"/> class.
        /// </summary>
        public XmlSelectorWrapper()
            : base(1)
        {
        }

        /// <summary>
        /// Gets the size of a single occurrence of this element, which is used to offset
        /// field positions for repeating segments and fields.
        /// </summary>
        /// <remarks>
        /// The concept of size is dependent on the stream format.  The size of an element in a fixed
        /// length stream format is determined by the length of the element in characters, while other
        /// stream formats calculate size based on the number of fields.  Some stream formats,
        /// such as XML, may ignore size settings.
        /// </remarks>
        public override int? Size
        {
            get { return ChildSelector.Size; }
        }

        /// <summary>
        /// Gets a value indicating whether this parser or any descendant of this parser is used to identify
        /// a record during unmarshalling.
        /// </summary>
        public override bool IsIdentifier
        {
            get { return ChildSelector.IsIdentifier; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Gets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        public override bool IsOptional
        {
            get { return ChildSelector.IsOptional; }
        }

        /// <summary>
        /// Gets the minimum number of occurrences of this component (within the context of its parent).
        /// </summary>
        public int MinOccurs
        {
            get { return ChildSelector.MinOccurs; }
        }

        /// <summary>
        /// Gets the maximum number of occurrences of this component (within the context of its parent).
        /// </summary>
        public int? MaxOccurs
        {
            get { return ChildSelector.MaxOccurs; }
        }

        /// <summary>
        /// Gets the order of this component (within the context of its parent).
        /// </summary>
        public int Order
        {
            get { return ChildSelector.Order; }
        }

        /// <summary>
        /// Gets the <see cref="IProperty"/> mapped to this component, or null if there is no property mapping.
        /// </summary>
        public IProperty Property
        {
            get { return ChildSelector.Property; }
        }

        /// <summary>
        /// Gets a value indicating whether this component is a record group.
        /// </summary>
        public bool IsRecordGroup
        {
            get { return false; }
        }

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
        /// Gets a value indicating whether this node is nillable.
        /// </summary>
        public bool IsNillable
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this node may repeat in the context of its immediate parent.
        /// </summary>
        public bool IsRepeating
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the child selector of this component wraps
        /// </summary>
        public ISelector ChildSelector
        {
            get { return (ISelector)First; }
        }

        public bool IsGroup { get; set; }

        public int Depth { get; set; }

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if matched, false otherwise</returns>
        public override bool Matches(UnmarshallingContext context)
        {
            // a group is never used to match a record
            return false;
        }

        /// <summary>
        /// Unmarshals a record
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise</returns>
        public override bool Unmarshal(UnmarshallingContext context)
        {
            return ChildSelector.Unmarshal(context);
        }

        /// <summary>
        /// Marshals a record
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/></param>
        /// <returns>whether a value was marshalled</returns>
        public override bool Marshal(MarshallingContext context)
        {
            var ctx = (XmlMarshallingContext)context;

            var parent = ctx.Parent;
            var node = new XElement(this.ToXName(true).ToConvertedName(ctx.NameConversionMode));
            parent.Add(node);
            if (IsGroup && ctx.IsStreaming)
            {
                node.AddAnnotation(new IsGroupElementAnnotation(true));
            }
            if (!IsNamespaceAware)
            {
                node.AddAnnotation(new NamespaceModeAnnotation(NamespaceHandlingMode.IgnoreNamespace));
            }
            else if (string.IsNullOrEmpty(Prefix))
            {
                node.AddAnnotation(new NamespaceModeAnnotation(NamespaceHandlingMode.DefaultNamespace));
            }
            else
            {
                node.SetAttributeValue(XNamespace.Xmlns + Prefix, Namespace);
            }

            ctx.Parent = node;
            var b = ChildSelector.Marshal(context);
            if (IsGroup && ctx.IsStreaming)
                ctx.CloseGroup(this);
            ctx.Parent = null;

            return b;
        }

        /// <summary>
        /// Returns whether this parser or any of its descendant have content for marshalling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>true if there is content for marshalling, false otherwise</returns>
        public override bool HasContent(ParsingContext context)
        {
            return ChildSelector.HasContent(context);
        }

        /// <summary>
        /// Clears the current property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        public override void ClearValue(ParsingContext context)
        {
            ChildSelector.ClearValue(context);
        }

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public override void SetValue(ParsingContext context, object value)
        {
            ChildSelector.SetValue(context, value);
        }

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public override object GetValue(ParsingContext context)
        {
            return ChildSelector.GetValue(context);
        }

        /// <summary>
        /// Returns the number of times this component was matched within the current
        /// iteration of its parent component.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the match count</returns>
        public int GetCount(ParsingContext context)
        {
            return ChildSelector.GetCount(context);
        }

        /// <summary>
        /// Sets the number of times this component was matched within the current
        /// iteration of its parent component.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="value">the new count</param>
        public void SetCount(ParsingContext context, int value)
        {
            ChildSelector.SetCount(context, value);
        }

        /// <summary>
        /// Returns a value indicating whether this component has reached its maximum occurrences
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>true if maximum occurrences has been reached</returns>
        public bool IsMaxOccursReached(ParsingContext context)
        {
            return ChildSelector.IsMaxOccursReached(context);
        }

        /// <summary>
        /// Finds a parser for marshalling a bean object
        /// </summary>
        /// <remarks>
        /// If matched by this Selector, the method
        /// should set the bean object on the property tree and return itself.
        /// </remarks>
        /// <param name="context">the <see cref="MarshallingContext"/></param>
        /// <returns>the matched <see cref="ISelector"/> for marshalling the bean object</returns>
        public ISelector MatchNext(MarshallingContext context)
        {
            var ctx = (XmlMarshallingContext)context;

            // stores the initial count before calling matchNext()...
            int initialCount = GetCount(context);

            var match = ChildSelector.MatchNext(context);
            if (match == null)
            {
                if (_written.Get(context))
                {
                    _written.Set(context, false);
                    ctx.CloseGroup(this);
                }
                return null;
            }

            if (IsGroup)
            {
                // if not marshalling to a stream, a group is always appended to the document by calling openGroup()
                if (ctx.IsStreaming)
                {
                    // if the group count increased, we need to close the current group
                    // element (by calling remove) and adding a new one
                    var w = _written.Get(context);
                    if (w && GetCount(context) > initialCount)
                    {
                        ctx.CloseGroup(this);
                        _written.Set(context, false);
                        w = false;
                    }
                    if (!w)
                    {
                        ctx.OpenGroup(this);
                        _written.Set(context, true);
                    }
                }
                else
                {
                    ctx.OpenGroup(this);
                }
                return match;
            }
            return this;
        }

        /// <summary>
        /// Finds a parser for unmarshalling a record based on the current state of the stream.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        /// <returns>the matched <see cref="ISelector"/> for unmarshalling the record</returns>
        public ISelector MatchNext(UnmarshallingContext context)
        {
            return Match(context, true);
        }

        /// <summary>
        /// Finds a parser that matches the input record
        /// </summary>
        /// <remarks>
        /// This method is invoked when <see cref="ISelector.MatchNext(BeanIO.Internal.Parser.UnmarshallingContext)"/> returns
        /// null, in order to differentiate between unexpected and unidentified record types.
        /// </remarks>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        /// <returns>the matched <see cref="ISelector"/></returns>
        public ISelector MatchAny(UnmarshallingContext context)
        {
            return Match(context, false);
        }

        /// <summary>
        /// Skips a record or group of records.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        public void Skip(UnmarshallingContext context)
        {
            ChildSelector.Skip(context);
        }

        /// <summary>
        /// Checks for any unsatisfied components before the stream is closed.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the first unsatisfied node</returns>
        public ISelector Close(ParsingContext context)
        {
            return ChildSelector.Close(context);
        }

        /// <summary>
        /// Resets the component count of this Selector's children.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        public void Reset(ParsingContext context)
        {
            _written.Set(context, false);
            ChildSelector.Reset(context);
        }

        /// <summary>
        /// Updates a <see cref="IDictionary{TKey,TValue}"/> with the current state of the Writer to allow for restoration at a later time.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="ns">a <see cref="string"/> to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> to update with the latest state</param>
        public void UpdateState(ParsingContext context, string ns, IDictionary<string, object> state)
        {
            state[GetKey(ns, WRITTEN_KEY)] = _written.Get(context);

            // allow children to update their state
            foreach (var node in Children.Cast<ISelector>())
                node.UpdateState(context, ns, state);
        }

        /// <summary>
        /// Restores a <see cref="IDictionary{TKey,TValue}"/> of previously stored state information
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="ns">a <see cref="string"/> to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> containing the state to restore</param>
        public void RestoreState(ParsingContext context, string ns, IReadOnlyDictionary<string, object> state)
        {
            var key = GetKey(ns, WRITTEN_KEY);
            var written = (bool)state[key];
            _written.Set(context, written);

            // allow children to restore their state
            foreach (var child in Children.Cast<ISelector>())
                child.RestoreState(context, ns, state);
        }

        /// <summary>
        /// Creates a DOM made up of all <see cref="XmlSelectorWrapper"/> descendants that wrap a group or record.
        /// </summary>
        /// <param name="nameConversionMode">The element and attribute name conversion mode</param>
        /// <returns>the created <see cref="XDocument"/></returns>
        public virtual XDocument CreateBaseDocument(ElementNameConversionMode nameConversionMode)
        {
            var doc = new XDocument();
            CreateBaseDocument(doc, this, nameConversionMode);
            return doc;
        }

        /// <summary>
        /// Called by a stream to register variables stored in the parsing context.
        /// </summary>
        /// <remarks>
        /// This method should be overridden by subclasses that need to register
        /// one or more parser context variables.
        /// </remarks>
        /// <param name="locals">set of local variables</param>
        public override void RegisterLocals(ISet<IParserLocal> locals)
        {
            if (locals.Add(_written))
                base.RegisterLocals(locals);
        }

        /// <summary>
        /// Returns a Map key for accessing state information for this Node
        /// </summary>
        /// <param name="ns">the assigned namespace for the key</param>
        /// <param name="name">the state information to access</param>
        /// <returns>the fully qualified key</returns>
        protected virtual string GetKey(string ns, string name)
        {
            return string.Format("{0}.{1}.{2}", ns, Name, name);
        }

        /// <summary>
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            s
                .AppendFormat(", depth={0}", Depth)
                .AppendFormat(", group={0}", IsGroup)
                .AppendFormat(", localName={0}", LocalName);
            if (!string.IsNullOrEmpty(Prefix))
                s.AppendFormat(", prefix={0}", Prefix);
            if (!string.IsNullOrEmpty(Namespace))
                s.AppendFormat(", xmlns={0}", IsNamespaceAware ? Namespace : "*");
        }

        private void CreateBaseDocument(XContainer parent, Component node, ElementNameConversionMode nameConversionMode)
        {
            var wrapper = node as XmlSelectorWrapper;
            if (wrapper != null)
            {
                if (!wrapper.IsGroup)
                    return;

                var element = new XElement(wrapper.ToXName(true).ToConvertedName(nameConversionMode));
                parent.Add(element);

                if (!wrapper.IsNamespaceAware)
                {
                    element.AddAnnotation(new NamespaceModeAnnotation(NamespaceHandlingMode.IgnoreNamespace));
                }
                else if (string.IsNullOrEmpty(Prefix))
                {
                    element.AddAnnotation(new NamespaceModeAnnotation(NamespaceHandlingMode.DefaultNamespace));
                }
                else
                {
                    element.SetAttributeValue(XNamespace.Xmlns + wrapper.Prefix, wrapper.Namespace);
                }

                parent = element;
            }

            foreach (var child in node.Children)
            {
                CreateBaseDocument(parent, child, nameConversionMode);
            }
        }

        /// <summary>
        /// Matches a child <see cref="ISelector"/>
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        /// <param name="stateful">whether to check the state of the matched child</param>
        /// <returns>the matched <see cref="ISelector"/>, or null if no match was made</returns>
        private ISelector Match(UnmarshallingContext context, bool stateful)
        {
            // validate the next element in the document matches this record
            var ctx = (XmlUnmarshallingContext)context;

            // update the position in the DOM tree (if null the node is matched)
            var matchedDomNode = ctx.PushPosition(this, Depth, IsGroup);
            if (matchedDomNode == null)
                return null;

            ISelector match = null;
            try
            {
                if (stateful)
                {
                    // get the number of times this node was read from the stream for comparing to our group count
                    var n = matchedDomNode.Annotation<GroupCountAnnotation>();
                    /*
                        if the group count is null, it means we expected a group and got a record, therefore no match
                        if (n == null) {
                            return null;
                        }
                    */
                    if (n != null && n.Count > GetCount(context))
                    {
                        if (IsMaxOccursReached(context))
                        {
                            return null;
                        }
                        SetCount(context, n.Count);
                        Reset(context);
                    }
                }

                // continue matching now that we've updated the DOM position...
                match = ChildSelector.MatchNext(context);

                return match;
            }
            finally
            {
                // if there was no match, reset the DOM position
                if (match == null)
                {
                    ctx.PopPosition();
                }
            }
        }
    }
}
