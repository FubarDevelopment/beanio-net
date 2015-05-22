using System;

namespace BeanIO.Types
{
    /// <summary>
    /// A type handler for <see cref="Guid"/> values.
    /// </summary>
    public class GuidTypeHandler : ITypeHandler
    {
        private readonly string _format = "D";

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidTypeHandler"/> class.
        /// </summary>
        public GuidTypeHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidTypeHandler"/> class.
        /// </summary>
        /// <param name="format">The <see cref="Guid"/> format to use</param>
        public GuidTypeHandler(string format)
        {
            _format = format;
        }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public Type TargetType
        {
            get { return typeof(Guid); }
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
            return Guid.Parse(text);
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
            return ((Guid)value).ToString(_format);
        }
    }
}
