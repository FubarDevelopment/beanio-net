using System;

namespace BeanIO.Types
{
    /// <summary>
    /// Type handler for the <see cref="string"/> type.
    /// </summary>
    public class StringTypeHandler : ITypeHandler
    {
        /// <summary>
        /// Gets or sets a value indicating whether to remove white spaces around the string value
        /// </summary>
        public bool Trim { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return <code>null</code> when the string value is empty
        /// </summary>
        public bool NullIfEmpty { get; set; }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public Type TargetType
        {
            get { return typeof(string); }
        }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public virtual object Parse(string text)
        {
            if (text == null)
                return null;

            if (Trim)
                text = text.Trim();

            if (NullIfEmpty && text.Length == 0)
                return null;

            return text;
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public virtual string Format(object value)
        {
            if (value == null)
                return null;
            return value.ToString();
        }
    }
}
