using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    public class Segment : ParserComponent
    {
        private readonly ParserLocal<IList<IParser>> _missing = new ParserLocal<IList<IParser>>(() => new List<IParser>());

        private bool _optional;

        private int _size;

        /// <summary>
        /// Gets or sets the <see cref="IProperty"/> mapped to this component, or null if there is no property mapping.
        /// </summary>
        public IProperty Property { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the existence is known to be true when unmarshal is called
        /// </summary>
        public bool IsExistencePredetermined { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the segment repeats
        /// </summary>
        public bool IsRepeating { get; set; }

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
            get { return _size; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this parser or any descendant of this parser is used to identify
        /// a record during unmarshalling.
        /// </summary>
        public override bool IsIdentifier { get; set; }

        /// <summary>
        /// Gets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        public override bool IsOptional
        {
            get { return _optional; }
        }

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if matched, false otherwise</returns>
        public override bool Matches(UnmarshallingContext context)
        {
            return !IsIdentifier || Children.Cast<IParser>().All(x => x.Matches(context));
        }

        /// <summary>
        /// Unmarshals a record
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise</returns>
        public override bool Unmarshal(UnmarshallingContext context)
        {
            var missing = _missing.Get(context);

            // unmarshals all children and determine existence,
            // if a child exists, the segment must exist
            // existence may also be predetermined in any tag based format (such as XML)
            var exists = IsExistencePredetermined;
            foreach (var parser in Children.Cast<IParser>())
            {
                if (parser.Unmarshal(context))
                {
                    exists = true;
                }
                else if (!parser.IsOptional)
                {
                    missing.Add(parser);
                }
            }

            // validate all required children are present if either the segment
            // exists or the segment itself is required
            if (exists || !IsOptional)
            {
                // validate there are no missing children
                if (missing.Count == 0)
                {
                    // if the segment valid and bound to a property, create the property value
                    if (Property != null)
                        Property.CreateValue(context);
                }
                else
                {
                    // otherwise create appropriate field errors for missing children
                    foreach (var parser in missing)
                    {
                        context.AddFieldError(parser.Name, null, "minOccurs", 1);
                    }
                }
            }

            missing.Clear();

            return exists;
        }

        /// <summary>
        /// Marshals a record
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/></param>
        /// <returns>whether a value was marshalled</returns>
        public override bool Marshal(MarshallingContext context)
        {
            if (IsOptional && !IsRepeating)
            {
                if (!HasContent(context))
                    return false;
            }

            foreach (var parser in Children.Cast<IParser>())
            {
                parser.Marshal(context);
            }

            return true;
        }

        /// <summary>
        /// Returns whether this parser or any of its descendant have content for marshalling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>true if there is content for marshalling, false otherwise</returns>
        public override bool HasContent(ParsingContext context)
        {
            if (Property != null)
                return !ReferenceEquals(Property.GetValue(context), Value.Missing);

            return Children.Cast<IParser>().Any(x => x.HasContent(context));
        }

        /// <summary>
        /// Clears the current property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        public override void ClearValue(ParsingContext context)
        {
            if (Property != null)
                Property.ClearValue(context);
        }

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public override void SetValue(ParsingContext context, object value)
        {
            if (Property != null)
                Property.SetValue(context, value);
        }

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public override object GetValue(ParsingContext context)
        {
            if (Property == null)
                return null;
            return Property.GetValue(context);
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
            if (Property != null)
                ((Component)Property).RegisterLocals(locals);

            if (locals.Add(_missing))
                base.RegisterLocals(locals);
        }

        /// <summary>
        /// Sets the size of a single occurrence of this element, which is used to offset
        /// field positions for repeating segments and fields.
        /// </summary>
        /// <param name="size">the size of a single occurrence of this element</param>
        public void SetSize(int size)
        {
            _size = size;
        }

        /// <summary>
        /// Sets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        /// <param name="optional">a value indicating whether this node must exist during unmarshalling</param>
        public void SetOptional(bool optional)
        {
            _optional = optional;
        }

        /// <summary>
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            s
                .AppendFormat(", size={0}", Size == null || Size == int.MaxValue ? "unbounded" : Size.ToString())
                .AppendFormat(", {0}", DebugUtil.FormatOption("rid", IsIdentifier))
                .AppendFormat(", {0}", DebugUtil.FormatOption("repeating", IsRepeating))
                .AppendFormat(", {0}", DebugUtil.FormatOption("optional", IsOptional));
            if (Property != null)
            {
                if (Property is Field)
                {
                    s.AppendFormat(", property=${0}", Property.Name);
                }
                else
                {
                    s.AppendFormat(", property={0}", Property);
                }
            }
        }
    }
}
