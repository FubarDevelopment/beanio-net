// <copyright file="DoubleTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Globalization;

namespace BeanIO.Types
{
    /// <summary>
    /// A type handler implementation for the <see cref="double"/> class.
    /// </summary>
    public class DoubleTypeHandler : NumberTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleTypeHandler"/> class.
        /// </summary>
        public DoubleTypeHandler()
            : base(typeof(double))
        {
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
            if (value is not IFormattable fmt)
                return value.ToString();
            if (Pattern is var (_, format))
            {
                return fmt.ToString(format, Culture);
            }

            var v = (double)value;
            return v.ToString(Culture);
        }

        /// <summary>
        /// Parses a string to a number by converting the text first to a decimal number and than
        /// to the target type.
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <param name="styles">The number styles to use.</param>
        /// <returns>The parsed number.</returns>
        protected override object Parse(string text, NumberStyles styles)
        {
            if (!double.TryParse(text, styles, Culture, out var result))
                throw new TypeConversionException($"Invalid {TargetType} value '{text}'");
            return result;
        }

        /// <summary>
        /// Parses a number from text.
        /// </summary>
        /// <param name="text">The text to convert to a number.</param>
        /// <returns>The parsed number.</returns>
        protected override object CreateNumber(string text)
        {
            return Parse(text, NumberStyles.Float);
        }
    }
}
