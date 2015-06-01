using System;
using System.Collections.Generic;

using BeanIO.Types;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// Default <see cref="Enum"/> type handler
    /// </summary>
    public class EnumTypeHandler : IConfigurableTypeHandler
    {
        private string _enumFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTypeHandler"/> class.
        /// </summary>
        /// <param name="enumType">The type derived from <see cref="Enum"/></param>
        public EnumTypeHandler(Type enumType)
        {
            TargetType = enumType;
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
        public object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            try
            {
                return Enum.Parse(TargetType, text, true);
            }
            catch (Exception ex)
            {
                throw new TypeConversionException(string.Format("Invalid {0} enum value '{1}'", TargetType.Name, text), ex);
            }
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
            if (string.IsNullOrEmpty(_enumFormat))
                return value.ToString();
            return Enum.Format(TargetType, value, _enumFormat);
        }

        /// <summary>
        /// Configures this type handler.
        /// </summary>
        /// <param name="properties">The properties for customizing the instance</param>
        public void Configure(IDictionary<string, string> properties)
        {
            string format;
            if (properties.TryGetValue("format", out format))
            {
                if (string.IsNullOrEmpty(format) || format == "name")
                    _enumFormat = "g";
                else if (string.Equals(format, "toString", StringComparison.OrdinalIgnoreCase))
                    _enumFormat = null;
                else
                    throw new BeanIOConfigurationException(string.Format("Invalid format '{0}', expected 'toString' or 'name' (default)", format));
            }
        }
    }
}
