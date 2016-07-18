// <copyright file="XmlDateTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Text;
using System.Xml;

using NodaTime;

namespace BeanIO.Types.Xml
{
    /// <summary>
    /// XML type handler for the <see cref="LocalDate"/> type.
    /// </summary>
    public class XmlDateTypeHandler : AbstractXmlDateTypeHandler
    {
        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public override Type TargetType => typeof(LocalDate);

        /// <summary>
        /// Gets the XML data type name
        /// </summary>
        protected override string DatatypeQName => "date";

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
            if (dto.Value.TimeOfDay != TimeSpan.Zero)
                throw new TypeConversionException($"Invalid XML {DatatypeQName} - no time component allowed");
            return ZonedDateTime.FromDateTimeOffset(dto.Value).Date;
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public override string Format(object value)
        {
            var ldt = (LocalDate?)value;
            if (ldt == null)
                return null;

            var dt = ldt.Value.AtMidnight().ToDateTimeUnspecified();
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
                var formatString = new StringBuilder("yyyy-MM-dd");
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
