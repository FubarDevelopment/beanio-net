using System;

namespace BeanIO.Types
{
    public class VersionTypeHandler : ITypeHandler
    {
        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public Type TargetType
        {
            get { return typeof(Version); }
        }

        public int? FieldCount { get; set; }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public virtual object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            return Version.Parse(text);
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
            var ver = (Version)value;
            if (FieldCount != null)
                return ver.ToString(FieldCount.Value);
            return ver.ToString();
        }
    }
}
