// <copyright file="AbstractXmlDateTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

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
        private static readonly string[] _defaultTimeFormats =
        {
            "H:mm",
            "H:mm",
            "H:mm:ss",
            "H:mm:ss.f",
            "H:mm:ss.ff",
            "H:mm:ss.fff",
            "H:mm:ss.ffff",
            "H:mm:ss.fffff",
            "H:mm:ss.ffffff",
            "HH:mm",
            "HH:mm:ss",
            "HH:mm:ss.f",
            "HH:mm:ss.ff",
            "HH:mm:ss.fff",
            "HH:mm:ss.ffff",
            "HH:mm:ss.fffff",
            "HH:mm:ss.ffffff",
        };

        private static readonly string[] _defaultTimeZoneFormats =
        {
            "z",
            "zz",
            "zzz",
            "zzzz",
            "zzzzz",
            "zzzzzz",
        };

        private string[]? _dateTimeOffsetFormatsNonLenient;

        private string[]? _dateTimeOffsetFormatsLenient;

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
        /// Defaults to <c>true</c>.
        /// </remarks>
        public bool IsTimeZoneAllowed { get; set; }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public override Type TargetType => typeof(DateTimeOffset);

        /// <summary>
        /// Gets the sequence of default time zone formats.
        /// </summary>
        protected static IEnumerable<string> DefaultTimeZoneFormats => _defaultTimeZoneFormats;

        /// <summary>
        /// Gets the sequence of default time formats.
        /// </summary>
        protected static IEnumerable<string> DefaultTimeFormats => _defaultTimeFormats;

        /// <summary>
        /// Gets the XML data type name.
        /// </summary>
        protected abstract string DatatypeQName { get; }

        /// <summary>
        /// Gets a value indicating whether the pattern contains a date.
        /// </summary>
        protected bool IsDatePattern
        {
            get
            {
                if (Pattern == null)
                    return DatatypeQName != "time";
                return Pattern.Contains("y")
                       || Pattern.Contains("d")
                       || Pattern.Contains("M")
                       || Pattern.Contains("/")
                       || Pattern.Contains("D")
                       || Pattern == "F"
                       || Pattern == "f"
                       || Pattern == "g"
                       || Pattern == "G"
                       || Pattern == "m"
                       || Pattern == "O"
                       || Pattern == "o"
                       || Pattern == "r"
                       || Pattern == "R"
                       || Pattern == "U"
                       || Pattern == "u"
                       || Pattern == "Y";
            }
        }

        private string[] DateTimeOffsetFormatsNonLenient => _dateTimeOffsetFormatsNonLenient ?? (_dateTimeOffsetFormatsNonLenient = CreateNonLenientFormats().ToArray());

        private string[] DateTimeOffsetFormatsLenient => _dateTimeOffsetFormatsLenient ?? (_dateTimeOffsetFormatsLenient = CreateLenientFormats().ToArray());

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record.</param>
        /// <returns>The parsed object.</returns>
        public override object? Parse(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            var replaceDate = false;
            DateTimeOffset dto;
            try
            {
                if (Pattern != null)
                {
                    dto = XmlConvert.ToDateTimeOffset(text, Pattern);
                    replaceDate = !IsDatePattern;
                }
                else
                {
                    try
                    {
                        dto = XmlConvert.ToDateTimeOffset(text, DateTimeOffsetFormatsNonLenient);
                    }
                    catch (FormatException)
                    {
                        var formats = DateTimeOffsetFormatsLenient;
                        if (!IsLenient || formats == null || formats.Length == 0)
                            throw;
                        dto = XmlConvert.ToDateTimeOffset(text, formats);
                        replaceDate = true;
                    }
                }
            }
            catch (FormatException ex)
            {
                throw new TypeConversionException($"Invalid XML {DatatypeQName}", ex);
            }

            if (replaceDate || string.Equals(DatatypeQName, "time", StringComparison.Ordinal))
                dto = new DateTimeOffset(new DateTime(1970, 1, 1) + dto.TimeOfDay, dto.Offset);
            if (!IsTimeZoneAllowed && dto.Offset != TimeSpan.Zero)
                throw new TypeConversionException($"Invalid XML {DatatypeQName}, time zone not allowed");
            return dto;
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null.</param>
        /// <returns>The formatted field text, or <see langword="null" /> to indicate the value is not present.</returns>
        public override string? Format(object? value)
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
        /// <param name="properties">The properties for customizing the instance.</param>
        public override void Configure(Properties properties)
        {
            base.Configure(properties);
            _dateTimeOffsetFormatsNonLenient = CreateNonLenientFormats().ToArray();
            _dateTimeOffsetFormatsLenient = CreateLenientFormats().ToArray();
        }

        /// <summary>
        /// Returns the time zone offset in minutes for the given date,
        /// or <see langword="null" /> if a time zone was not configured.
        /// </summary>
        /// <param name="date">the date on which to determine the time zone offset.</param>
        /// <returns>the time zone offset in minutes, or <see langword="null" />.</returns>
        protected TimeSpan? GetTimeZoneOffset(ZonedDateTime date)
        {
            if (TimeZone == null)
                return null;
            return TimeSpan.FromMilliseconds(date.Zone.GetUtcOffset(date.ToInstant()).Milliseconds);
        }

        /// <summary>
        /// Creates a sequence of non-lenient XML date (time) formats.
        /// </summary>
        /// <returns>a sequence of non-lenient XML date (time) formats.</returns>
        protected virtual IEnumerable<string> CreateNonLenientFormats()
        {
            foreach (var timeComponent in _defaultTimeFormats)
            {
                foreach (var timezoneComponent in _defaultTimeZoneFormats)
                {
                    yield return $"yyyy-MM-ddT{timeComponent}{timezoneComponent}";
                }
            }

            foreach (var timeComponent in _defaultTimeFormats)
            {
                yield return $"yyyy-MM-ddT{timeComponent}";
            }

            foreach (var timezoneComponent in _defaultTimeZoneFormats)
            {
                yield return $"yyyy-MM-dd{timezoneComponent}";
            }

            yield return "yyyy-MM-dd";
        }

        /// <summary>
        /// Returns a sequence of lenient XML date (time) formats.
        /// </summary>
        /// <returns>a sequence of lenient XML date (time) formats.</returns>
        protected virtual IEnumerable<string> CreateLenientFormats()
        {
            foreach (var timeComponent in _defaultTimeFormats)
            {
                foreach (var timezoneComponent in _defaultTimeZoneFormats)
                {
                    yield return $"{timeComponent}{timezoneComponent}";
                }
            }

            foreach (var timeComponent in _defaultTimeFormats)
            {
                yield return $"{timeComponent}";
            }
        }
    }
}
