using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using BeanIO.Config;

using NodaTime;

namespace BeanIO.Types.Xml
{
    /// <summary>
    /// Base class for <see cref="ZonedDateTime"/> type handlers based on the W3C XML Schema
    /// data type specification.
    /// </summary>
    public abstract class AbstractXmlDateTypeHandler : DateTypeHandlerSupport
    {
        private string[] _dateTimeOffsetFormats;

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

        private string[] DateTimeOffsetFormats
        {
            get { return _dateTimeOffsetFormats ?? (_dateTimeOffsetFormats = CreateFormats(IsLenient).ToArray()); }
        }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public override object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            DateTimeOffset dto;
            try
            {
                if (Pattern != null)
                {
                    dto = XmlConvert.ToDateTimeOffset(text, Pattern);
                }
                else
                {
                    dto = XmlConvert.ToDateTimeOffset(text, DateTimeOffsetFormats);
                }
                if (!text.Contains("-"))
                    dto = new DateTimeOffset(new DateTime(1970, 1, 1) + dto.TimeOfDay, dto.Offset);
            }
            catch (FormatException ex)
            {
                throw new TypeConversionException(string.Format("Invalid XML {0}", DatatypeQName), ex);
            }
            if (!IsTimeZoneAllowed && dto.Offset != TimeSpan.Zero)
                throw new TypeConversionException(string.Format("Invalid XML {0}, time zone not allowed", DatatypeQName));
            return dto;
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
        /// Configures this type handler.
        /// </summary>
        /// <param name="properties">The properties for customizing the instance</param>
        public override void Configure(Properties properties)
        {
            base.Configure(properties);
            _dateTimeOffsetFormats = CreateFormats(IsLenient).ToArray();
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

        private static IEnumerable<string> CreateFormats(bool lenient)
        {
            var timeComponents = new[]
                {
                    string.Empty,
                    "H:mm",
                    "HH:mm",
                    "HH:mm:ss",
                    "HH:mm:ss.f",
                    "HH:mm:ss.ff",
                    "HH:mm:ss.fff",
                    "HH:mm:ss.ffff",
                    "HH:mm:ss.fffff",
                    "HH:mm:ss.ffffff",
                };
            var timezoneComponents = new[]
                {
                    string.Empty,
                    "z",
                    "zz",
                    "zzz",
                    "zzzz",
                    "zzzzz",
                    "zzzzzz",
                    "zzzzzzz",
                };
            foreach (var timeComponent in timeComponents)
            {
                foreach (var timezoneComponent in timezoneComponents)
                {
                    yield return string.Format(
                        "yyyy-MM-dd{2}{0}{1}",
                        timeComponent,
                        timezoneComponent,
                        !string.IsNullOrEmpty(timeComponent) ? "T" : string.Empty);
                }
            }
            if (lenient)
            {
                foreach (var timeComponent in timeComponents.Where(x => !string.IsNullOrEmpty(x)))
                {
                    foreach (var timezoneComponent in timezoneComponents)
                    {
                        var formatString = string.Format(
                            "{0}{1}",
                            timeComponent,
                            timezoneComponent);
                        yield return formatString;
                    }
                }
            }
        }
    }
}
