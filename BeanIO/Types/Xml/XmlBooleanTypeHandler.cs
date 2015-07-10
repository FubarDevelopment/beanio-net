using System;
using System.Xml;

using BeanIO.Config;

namespace BeanIO.Types.Xml
{
    /// <summary>
    /// XML type handler for the <see cref="bool"/> type.
    /// </summary>
    public class XmlBooleanTypeHandler : XmlConvertTypeHandler, IConfigurableTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlBooleanTypeHandler"/> class.
        /// </summary>
        public XmlBooleanTypeHandler()
            : base(typeof(bool), null, t => XmlConvert.ToBoolean(t))
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the formatted value should be numeric
        /// </summary>
        public bool IsNumericFormatEnabled { get; set; }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public override string Format(object value)
        {
            if (value == null)
                return null;
            var v = (bool)value;
            if (IsNumericFormatEnabled)
                return v ? "1" : "0";
            return v ? "true" : "false";
        }

        /// <summary>
        /// Configures this type handler.
        /// </summary>
        /// <param name="properties">The properties for customizing the instance</param>
        public virtual void Configure(Properties properties)
        {
            var v = properties["numeric"];
            IsNumericFormatEnabled = !string.IsNullOrEmpty(v) && XmlConvert.ToBoolean(v);
        }
    }
}
