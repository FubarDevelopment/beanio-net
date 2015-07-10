using System;

namespace BeanIO.Types
{
    /// <summary>
    /// Type handler for <see cref="bool"/> which uses a character representation.
    /// </summary>
    public class BooleanCharacterTypeHandler : ITypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanCharacterTypeHandler"/> class.
        /// </summary>
        public BooleanCharacterTypeHandler()
        {
            TrueValue = 'Y';
            FalseValue = 'N';
        }

        /// <summary>
        /// Gets or sets the character to be used for <code>true</code>.
        /// </summary>
        public char TrueValue { get; set; }

        /// <summary>
        /// Gets or sets the character to be used for <code>false</code>.
        /// </summary>
        public char? FalseValue { get; set; }

        /// <summary>
        /// Gets or sets the character to be used for <code>null</code>
        /// </summary>
        public char? NullValue { get; set; }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public Type TargetType
        {
            get { return typeof(bool); }
        }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public virtual object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            if (text.Length != 1)
                throw new FormatException(string.Format("Invalid value '{0}' (too long)", text));

            var ch = text[0];
            if (ch == TrueValue)
                return true;
            if (FalseValue.HasValue && FalseValue == ch)
                return false;

            throw new FormatException(string.Format("Invalid value '{0}' for type '{1}'", text, TargetType.Name));
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public virtual string Format(object value)
        {
            if (value == null)
                return string.Format("{0}", NullValue);
            var boolValue = (bool)value;
            return string.Format("{0}", boolValue ? TrueValue : FalseValue);
        }
    }
}
