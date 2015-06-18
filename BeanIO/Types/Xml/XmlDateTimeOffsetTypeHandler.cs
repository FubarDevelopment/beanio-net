using System;
using System.Text;
using System.Xml;

namespace BeanIO.Types.Xml
{
    public class XmlDateTimeOffsetTypeHandler : AbstractXmlDateTypeHandler
    {
        /// <summary>
        /// Gets or sets a value indicating whether milliseconds are included when formatting the time
        /// </summary>
        public bool OutputMilliseconds { get; set; }

        protected override string DatatypeQName
        {
            get { return "dateTime"; }
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public override string Format(object value)
        {
            var dto = (DateTimeOffset?)value;
            if (dto == null)
                return null;

            string pattern;
            if (Pattern == null)
            {
                var formatString = new StringBuilder("yyyy-MM-ddTHH:mm:ss");
                if (OutputMilliseconds)
                    formatString.Append(".fff");
                if (TimeZone != null)
                    formatString.Append("K");
                pattern = formatString.ToString();
            }
            else
            {
                pattern = Pattern;
            }

            return XmlConvert.ToString(dto.Value, pattern);
        }
    }
}
