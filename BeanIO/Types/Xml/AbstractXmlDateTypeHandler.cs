using System;
using System.Xml;

using NodaTime;

namespace BeanIO.Types.Xml
{
    /// <summary>
    /// Base class for <see cref="ZonedDateTime"/> type handlers based on the W3C XML Schema
    /// data type specification.
    /// </summary>
    public abstract class AbstractXmlDateTypeHandler : DateTypeHandlerSupport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractXmlDateTypeHandler"/> class.
        /// </summary>
        protected AbstractXmlDateTypeHandler()
        {
            IsTimeZoneAllowed = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether time zone information is allowed when parsing field text.
        /// </summary>
        /// <remarks>
        /// Defaults to <code>true</code>
        /// </remarks>
        public bool IsTimeZoneAllowed { get; set; }

        public override Type TargetType
        {
            get { return typeof(DateTimeOffset); }
        }

        protected abstract string DatatypeQName { get; }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public override object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            if (Pattern != null)
                return XmlConvert.ToDateTimeOffset(text, Pattern);
            return XmlConvert.ToDateTimeOffset(text);
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public override string Format(object value)
        {
            if (value == null)
                return null;
            var dto = (DateTimeOffset)value;
            if (Pattern != null)
                return XmlConvert.ToString(dto, Pattern);
            return XmlConvert.ToString(dto);
        }

        /// <summary>
        /// Returns the time zone offset in minutes for the given date,
        /// or <code>null</code> if a time zone was not configured.
        /// </summary>
        /// <param name="date">the date on which to determine the time zone offset</param>
        /// <returns>the time zone offset in minutes, or <code>null</code></returns>
        protected TimeSpan? GetTimeZoneOffset(ZonedDateTime date)
        {
            if (TimeZone == null)
                return null;
            return TimeSpan.FromMilliseconds(date.Zone.GetUtcOffset(date.ToInstant()).Milliseconds);
        }
    }
}
