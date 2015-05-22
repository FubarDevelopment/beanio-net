using System;
using System.Collections.Generic;

using NodaTime;
using NodaTime.Text;

namespace BeanIO.Types
{
    /// <summary>
    /// This abstract type handler uses a <see cref="LocalDateTimePattern"/> class to parse and format
    /// <see cref="LocalDateTime"/> objects.
    /// </summary>
    public abstract class DateTypeHandlerSupport : CultureSupport, IConfigurableTypeHandler
    {
        private string _pattern;

        private LocalDateTimePattern _format;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTypeHandlerSupport"/> class.
        /// </summary>
        protected DateTypeHandlerSupport()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTypeHandlerSupport"/> class.
        /// </summary>
        /// <param name="pattern">The pattern to use for parsing and formatting</param>
        protected DateTypeHandlerSupport(string pattern)
        {
            Pattern = pattern;
        }

        /// <summary>
        /// Gets or sets the time zone for interpreting dates.
        /// </summary>
        /// <remarks>
        /// If not set, the UTC time zone is used.
        /// </remarks>
        public DateTimeZone TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the time zone ID for interpreting dates.
        /// </summary>
        /// <remarks>
        /// If not set, the UTC time zone is used.
        /// </remarks>
        public string TimeZoneId
        {
            get { return TimeZone == null ? null : TimeZone.Id; }
            set { TimeZone = string.IsNullOrEmpty(value) ? null : DateTimeZoneProviders.Tzdb[value]; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the time zone is set leniently.
        /// </summary>
        public bool IsLenient { get; set; }

        /// <summary>
        /// Gets or sets the date/time pattern used by the <see cref="LocalDateTimePattern"/>.
        /// </summary>
        public string Pattern
        {
            get
            {
                return _pattern;
            }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                        LocalDateTimePattern.Create(value, Culture);
                    _pattern = value;
                }
                catch (InvalidPatternException ex)
                {
                    throw new ArgumentException(string.Format("Invalid date format pattern '{0}': {1}", value, ex.Message), ex);
                }
            }
        }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public abstract Type TargetType { get; }

        private LocalDateTimePattern DateFormat
        {
            get { return _format ?? (_format = CreateDateFormat()); }
        }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public abstract object Parse(string text);

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public abstract string Format(object value);

        /// <summary>
        /// Configures this type handler.
        /// </summary>
        /// <param name="properties">The properties for customizing the instance</param>
        public virtual void Configure(IDictionary<string, string> properties)
        {
            var pattern = properties["format"];
            if (string.IsNullOrEmpty(pattern))
                return;

            if (string.Equals(pattern, Pattern, StringComparison.Ordinal))
                return;

            Pattern = pattern;
            _format = null;
        }

        /// <summary>
        /// Parses the local date (and/or time) into a <see cref="ZonedDateTime"/>.
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <returns>The parsed date (and/or time)</returns>
        protected ZonedDateTime? ParseDate(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var dt = DateFormat.Parse(text).GetValueOrThrow();

            var tz = TimeZone ?? DateTimeZone.Utc;
            var result = IsLenient ? dt.InZoneLeniently(tz) : dt.InZoneStrictly(tz);

            return result;
        }

        /// <summary>
        /// Formats a local date using the given pattern.
        /// </summary>
        /// <param name="date">The <see cref="LocalDateTime"/> to format</param>
        /// <returns>null or the formatted <paramref name="date"/></returns>
        protected string FormatDate(LocalDateTime? date)
        {
            if (date == null)
                return null;
            return DateFormat.Format(date.Value);
        }

        /// <summary>
        /// Formats a local date using the given pattern.
        /// </summary>
        /// <param name="date">The <see cref="ZonedDateTime"/> to format</param>
        /// <returns>null or the formatted <paramref name="date"/></returns>
        protected string FormatDate(ZonedDateTime? date)
        {
            if (date == null)
                return null;
            return DateFormat.Format(date.Value.LocalDateTime);
        }

        /// <summary>
        /// Creates a <see cref="LocalDateTimePattern"/>
        /// </summary>
        /// <returns>The created <see cref="LocalDateTimePattern"/></returns>
        protected virtual LocalDateTimePattern CreateDateFormat()
        {
            if (string.IsNullOrEmpty(_pattern))
                return CreateDefaultDateFormat();
            return LocalDateTimePattern.Create(_pattern, Culture);
        }

        /// <summary>
        /// Creates a default <see cref="LocalDateTimePattern"/> when no <see cref="Pattern"/> is specified.
        /// </summary>
        /// <returns>The created <see cref="LocalDateTimePattern"/></returns>
        protected virtual LocalDateTimePattern CreateDefaultDateFormat()
        {
            return LocalDateTimePattern.Create("d", Culture);
        }
    }
}
