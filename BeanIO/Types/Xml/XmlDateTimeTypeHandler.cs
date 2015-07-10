using System;
using System.Text;
using System.Xml;

using NodaTime;

namespace BeanIO.Types.Xml
{
    /// <summary>
    /// XML type handler for the <see cref="DateTime"/> type.
    /// </summary>
    public class XmlDateTimeTypeHandler : AbstractXmlDateTypeHandler
    {
        /// <summary>
        /// Gets or sets a value indicating whether milliseconds are included when formatting the time
        /// </summary>
        public bool OutputMilliseconds { get; set; }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public override Type TargetType
        {
            get { return typeof(DateTime); }
        }

        /// <summary>
        /// Gets the XML data type name
        /// </summary>
        protected override string DatatypeQName
        {
            get { return "dateTime"; }
        }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public override object Parse(string text)
        {
            var dto = (DateTimeOffset?)base.Parse(text);
            if (dto == null)
                return null;
            return dto.Value.DateTime;
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public override string Format(object value)
        {
            var dt = (DateTime?)value;
            if (dt == null)
                return null;

            DateTimeOffset dto;
            if (TimeZone != null)
            {
                var instant = Instant.FromDateTimeUtc(dt.Value);
                var offset = TimeZone.GetUtcOffset(instant);
                dto = new DateTimeOffset(dt.Value, TimeSpan.FromMilliseconds(offset.Milliseconds));
            }
            else
            {
                dto = new DateTimeOffset(dt.Value);
            }

            string pattern;
            if (Pattern == null)
            {
                var formatString = new StringBuilder("yyyy-MM-ddTHH:mm:ss");
                if (OutputMilliseconds)
                    formatString.Append(".fffffff");
                if (TimeZone != null)
                    formatString.Append("K");
                pattern = formatString.ToString();
            }
            else
            {
                pattern = Pattern;
            }

            return XmlConvert.ToString(dto, pattern);
        }
    }
}
