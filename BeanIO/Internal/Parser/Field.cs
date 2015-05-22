using System;
using System.Text.RegularExpressions;

using BeanIO.Internal.Util;
using BeanIO.Types;

namespace BeanIO.Internal.Parser
{
    public class Field : ParserComponent, IProperty
    {
        private bool _isIdentifier;

        private Regex _regex;

        public Field()
            : base(0)
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
        public override int Size
        {
            get { return Format.Size; }
        }

        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public PropertyType Type
        {
            get { return Parser.PropertyType.Simple; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this property or any of its descendants are used to identify a bean object.
        /// </summary>
        bool IProperty.IsIdentifier
        {
            get { return _isIdentifier; }
            set { _isIdentifier = value; }
        }

        /// <summary>
        /// Gets or sets the property accessor
        /// </summary>
        public IPropertyAccessor Accessor { get; set; }

        /// <summary>
        /// Gets or sets the bean property type
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this parser or any descendant of this parser is used to identify
        /// a record during unmarshalling.
        /// </summary>
        public override bool IsIdentifier
        {
            get { return _isIdentifier; }
        }

        /// <summary>
        /// Gets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        public override bool IsOptional
        {
            get { return Format.IsLazy; }
        }

        public IFieldFormat Format { get; set; }

        public bool IsBound { get; set; }

        public string Regex
        {
            get { return _regex == null ? null : _regex.ToString(); }
            set { _regex = value == null ? null : new Regex(value); }
        }

        public string Literal { get; set; }

        public bool IsTrim { get; set; }

        public bool IsRequired { get; set; }

        public bool IsLazy { get; set; }

        public int MinLength { get; set; }

        public int? MaxLength { get; set; }

        public string DefaultValue { get; set; }

        public ITypeHandler Handler { get; set; }

        protected Regex RegexPattern
        {
            get { return _regex; }
        }

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if matched, false otherwise</returns>
        public override bool Matches(UnmarshallingContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unmarshals a record
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise</returns>
        public override bool Unmarshal(UnmarshallingContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Marshals a record
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/></param>
        /// <returns>whether a value was marshalled</returns>
        public override bool Marshal(MarshallingContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns whether this parser or any of its descendant have content for marshalling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>true if there is content for marshalling, false otherwise</returns>
        public override bool HasContent(ParsingContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears the current property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        public override void ClearValue(ParsingContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the property value and returns it
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public object CreateValue(ParsingContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public override void SetValue(ParsingContext context, object value)
        {
            throw new NotImplementedException();
        }

        public bool Defines(object value)
        {
            if (value == null)
                return false;
            if (!IsIdentifier)
                return true;

            if (!value.GetType().IsInstanceOf(PropertyType))
                return false;

            return IsMatch(FormatValue(value));
        }

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public override object GetValue(ParsingContext context)
        {
            throw new NotImplementedException();
        }

        protected virtual bool IsMatch(string text)
        {
            if (text == null)
                return false;
            if (Literal != null && !string.Equals(Literal, text, StringComparison.Ordinal))
                return false;
            if (RegexPattern != null && !RegexPattern.IsMatch(text))
                return false;
            return true;
        }

        protected virtual string FormatValue(object value)
        {
            if (value == Value.Nil || value == Value.Missing)
                return null;

            string text;
            if (Handler != null)
            {
                try
                {
                    
                }
            }
        }
    }
}
