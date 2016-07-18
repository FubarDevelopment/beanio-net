// <copyright file="XmlTimeTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NodaTime;

namespace BeanIO.Types.Xml
{
    /// <summary>
    /// XML type handler for the <see cref="LocalTime"/> type.
    /// </summary>
    public class XmlTimeTypeHandler : AbstractXmlDateTypeHandler
    {
        /// <summary>
        /// Gets or sets a value indicating whether milliseconds are included when formatting the time
        /// </summary>
        public bool OutputMilliseconds { get; set; }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public override Type TargetType => typeof(LocalTime);

        /// <summary>
        /// Gets the XML data type name
        /// </summary>
        protected override string DatatypeQName => "time";

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
            return ZonedDateTime.FromDateTimeOffset(dto.Value).TimeOfDay;
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public override string Format(object value)
        {
            var lt = (LocalTime?)value;
            if (lt == null)
                return null;

            var dt = LocalDate.FromDateTime(DateTime.Now).At(lt.Value).ToDateTimeUnspecified();
            DateTimeOffset dto;
            if (TimeZone != null)
            {
                var instant = Instant.FromDateTimeUtc(dt);
                var offset = TimeZone.GetUtcOffset(instant);
                dto = new DateTimeOffset(dt, TimeSpan.FromMilliseconds(offset.Milliseconds));
            }
            else
            {
                dto = new DateTimeOffset(dt);
            }

            string pattern;
            if (Pattern == null)
            {
                var formatString = new StringBuilder("HH:mm:ss");
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

            return XmlConvert.ToString(dto, pattern);
        }

        /// <summary>
        /// Creates a sequence of non-lenient XML date (time) formats.
        /// </summary>
        /// <returns>a sequence of non-lenient XML date (time) formats</returns>
        protected override IEnumerable<string> CreateNonLenientFormats()
        {
            foreach (var defaultTimeFormat in DefaultTimeFormats)
            {
                foreach (var defaultTimeZoneFormat in DefaultTimeZoneFormats)
                {
                    yield return $"{defaultTimeFormat}{defaultTimeZoneFormat}";
                }
            }

            foreach (var defaultTimeFormat in DefaultTimeFormats)
            {
                yield return defaultTimeFormat;
            }
        }

        /// <summary>
        /// Returns a sequence of lenient XML date (time) formats
        /// </summary>
        /// <returns>a sequence of lenient XML date (time) formats</returns>
        protected override IEnumerable<string> CreateLenientFormats()
        {
            return new string[0];
        }
    }
}
