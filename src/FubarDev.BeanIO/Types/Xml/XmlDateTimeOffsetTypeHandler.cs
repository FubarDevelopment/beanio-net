// <copyright file="XmlDateTimeOffsetTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Text;
using System.Xml;

namespace BeanIO.Types.Xml
{
    /// <summary>
    /// XML type handler for the <see cref="DateTimeOffset"/> type.
    /// </summary>
    public class XmlDateTimeOffsetTypeHandler : AbstractXmlDateTypeHandler
    {
        /// <summary>
        /// Gets or sets a value indicating whether milliseconds are included when formatting the time.
        /// </summary>
        public bool OutputMilliseconds { get; set; }

        /// <summary>
        /// Gets the XML data type name.
        /// </summary>
        protected override string DatatypeQName => "dateTime";

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null.</param>
        /// <returns>The formatted field text, or <see langword="null" /> to indicate the value is not present.</returns>
        public override string? Format(object? value)
        {
            var dto = (DateTimeOffset?)value;
            if (dto == null)
                return null;

            string pattern;
            if (Pattern == null)
            {
                var formatString = new StringBuilder("yyyy-MM-dd");
                if (OutputMilliseconds || dto.Value.TimeOfDay != TimeSpan.Zero)
                {
                    formatString.Append("THH:mm:ss");
                    if (OutputMilliseconds)
                        formatString.Append(".fff");
                }

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
