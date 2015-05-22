using System;

namespace BeanIO.Types
{
    public class BooleanTypeHandler : ITypeHandler
    {
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
        public object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            return bool.Parse(text.Trim());
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public string Format(object value)
        {
            if (value == null)
                return null;
            return value.ToString();
        }
    }
}
