using System;

namespace BeanIO.Types.Xml
{
    /// <summary>
    /// An type handler that uses the <see cref="System.Xml.XmlConvert"/> function.
    /// </summary>
    public class XmlConvertTypeHandler : ITypeHandler
    {
        private readonly Func<object, string> _formatFunc;

        private readonly Func<string, object> _parseFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlConvertTypeHandler"/> class.
        /// </summary>
        /// <param name="targetType">the target type of this type handler</param>
        /// <param name="formatFunc">the function to format the value to a <see cref="string"/></param>
        /// <param name="parseFunc">the function that parses a <see cref="string"/></param>
        public XmlConvertTypeHandler(Type targetType, Func<object, string> formatFunc, Func<string, object> parseFunc)
        {
            TargetType = targetType;
            _formatFunc = formatFunc;
            _parseFunc = parseFunc;
        }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public Type TargetType { get; private set; }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public virtual object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            return _parseFunc(text);
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
            return _formatFunc(value);
        }
    }
}
