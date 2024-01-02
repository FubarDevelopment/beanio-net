// <copyright file="BooleanIntegerTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Globalization;

using BeanIO.Config;

namespace BeanIO.Types
{
    /// <summary>
    /// Converts a <see cref="bool"/> to an <see cref="int"/> value.
    /// </summary>
    public class BooleanIntegerTypeHandler : CultureSupport, IConfigurableTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanIntegerTypeHandler"/> class.
        /// </summary>
        public BooleanIntegerTypeHandler()
        {
            TrueValue = 1;
            FalseValue = 0;
        }

        /// <summary>
        /// Gets or sets the <see cref="int"/> value to be used for <c>true</c>.
        /// </summary>
        public int TrueValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="int"/> value to be used for <see langword="false" />.
        /// </summary>
        public int? FalseValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="int"/> value to be used for <see langword="null" />.
        /// </summary>
        public int? NullValue { get; set; }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public Type TargetType => typeof(bool);

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record.</param>
        /// <returns>The parsed object.</returns>
        public virtual object? Parse(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            const NumberStyles styles = NumberStyles.Integer;
            if (!int.TryParse(text, styles, Culture, out var intValue))
                throw new FormatException($"Number value '{text}' doesn't match the number styles {styles}");

            if (intValue == TrueValue)
                return true;
            if (FalseValue.HasValue && FalseValue == intValue)
                return false;

            throw new FormatException($"Invalid value '{text}' for type '{TargetType.Name}'");
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null.</param>
        /// <returns>The formatted field text, or <see langword="null" /> to indicate the value is not present.</returns>
        public virtual string Format(object? value)
        {
            if (value == null)
                return string.Format(Culture, "{0}", NullValue);
            var boolValue = (bool)value;
            return string.Format(Culture, "{0}", boolValue ? TrueValue : FalseValue);
        }

        /// <inheritdoc />
        public virtual void Configure(Properties properties)
        {
        }
    }
}
