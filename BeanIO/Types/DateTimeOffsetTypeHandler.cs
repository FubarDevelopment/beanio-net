// <copyright file="DateTimeOffsetTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using NodaTime;

namespace BeanIO.Types
{
    /// <summary>
    /// The type handler for <see cref="DateTimeOffset"/>.
    /// </summary>
    public class DateTimeOffsetTypeHandler : DateTypeHandlerSupport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeOffsetTypeHandler"/> class.
        /// </summary>
        public DateTimeOffsetTypeHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeOffsetTypeHandler"/> class.
        /// </summary>
        /// <param name="pattern">The pattern to use</param>
        public DateTimeOffsetTypeHandler(string pattern)
            : base(pattern)
        {
        }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public override Type TargetType => typeof(DateTimeOffset);

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public override object Parse(string text)
        {
            var dt = ParseDate(text);
            return dt?.ToDateTimeOffset();
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
            var dt = (DateTimeOffset)value;
            return FormatDate(ZonedDateTime.FromDateTimeOffset(dt));
        }
    }
}
