// <copyright file="NumberTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Globalization;

using BeanIO.Config;

namespace BeanIO.Types
{
    /// <summary>
    /// A type handler implementation for the <see cref="decimal"/> class.
    /// </summary>
    /// <remarks>
    /// If <see cref="Pattern"/> is set, a NumberStyle is used to parse the value and a format string to format the value.
    /// Otherwise, the value is parsed and formatted using the <see cref="decimal"/> class.
    /// </remarks>
    public class NumberTypeHandler : CultureSupport, IConfigurableTypeHandler
    {
        private (NumberStyles Styles, string Format)? _pattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberTypeHandler"/> class.
        /// </summary>
        /// <param name="numberType">The number type to convert from/to.</param>
        public NumberTypeHandler(Type numberType)
        {
            if (numberType == null)
                throw new ArgumentNullException(nameof(numberType));
            if (!numberType.IsValueType)
                throw new ArgumentOutOfRangeException(nameof(numberType));
            TargetType = numberType;
        }

        /// <summary>
        /// Gets or sets the format pattern to use to parse and format the number value.
        /// </summary>
        public (NumberStyles Styles, string Format)? Pattern
        {
            get => _pattern;
            set =>
                _pattern = value switch
                {
                    var (_, format) when string.IsNullOrEmpty(format) =>
                        throw new ArgumentOutOfRangeException(nameof(value)),
                    _ => value,
                };
        }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public virtual Type TargetType { get; private set; }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record.</param>
        /// <returns>The parsed object.</returns>
        public virtual object? Parse(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            if (Pattern is var (styles, _))
                return Parse(text!, styles);

            return CreateNumber(text!);
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null.</param>
        /// <returns>The formatted field text, or <see langword="null" /> to indicate the value is not present.</returns>
        public virtual string? Format(object? value)
        {
            if (value == null)
                return null;
            if (value is not IFormattable fmt)
                return value.ToString();
            if (Pattern is var (_, format))
            {
                return fmt.ToString(format, Culture);
            }

            var dec = Convert.ToDecimal(value);
            return dec.ToString(Culture);
        }

        /// <summary>
        /// Configures this type handler.
        /// </summary>
        /// <param name="properties">The properties for customizing the instance.</param>
        public virtual void Configure(Properties properties)
        {
            if (properties.TryGetValue(DefaultTypeConfigurationProperties.FORMAT_SETTING, out var formatSetting))
            {
                if (!string.IsNullOrEmpty(formatSetting))
                {
                    var parts = formatSetting.Split(new[] { ';' }, 2);
                    var format = parts[0];
                    var stylesAsString = parts.Length == 2 ? parts[1] : string.Empty;
                    NumberStyles styles;
                    if (string.IsNullOrEmpty(stylesAsString))
                    {
                        styles = NumberStyles.Number;
                        if (format.IndexOf(',') != -1)
                            styles |= NumberStyles.AllowThousands;
                        if (format.IndexOf('.') != -1)
                            styles |= NumberStyles.AllowDecimalPoint;
                        if (format.IndexOfAny(new[] { 'e', 'E' }) != -1)
                            styles |= NumberStyles.AllowExponent;
                        if (format.IndexOf('-') != -1)
                            styles |= NumberStyles.AllowLeadingSign;
                        if (format.IndexOfAny(new[] { '(', ')' }) != -1)
                            styles |= NumberStyles.AllowParentheses;
                        if (format.IndexOf('$') != -1 || format.IndexOf(Culture.NumberFormat.CurrencySymbol, StringComparison.CurrentCultureIgnoreCase) != -1)
                            styles |= NumberStyles.Currency;
                        var hasDigitPlaceholders = format.IndexOfAny(new[] { '0', '#' }) != -1;
                        if (!hasDigitPlaceholders && styles == NumberStyles.Number && format.IndexOfAny(new[] { 'x', 'X' }) != -1)
                            styles |= NumberStyles.AllowHexSpecifier;
                    }
                    else
                    {
                        styles = (NumberStyles)Enum.Parse(typeof(NumberStyles), stylesAsString, true);
                    }

                    Pattern = (styles, format);
                }
            }

            if (properties.TryGetValue(DefaultTypeConfigurationProperties.NUMBER_TYPE_SETTING, out var numberType))
            {
                if (!string.IsNullOrEmpty(numberType))
                {
                    TargetType = Type.GetType(numberType, true)!;
                }
            }
        }

        /// <summary>
        /// Parses a string to a number by converting the text first to a decimal number and than
        /// to the target type.
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <param name="styles">The number styles to use.</param>
        /// <returns>The parsed number.</returns>
        protected virtual object Parse(string text, NumberStyles styles)
        {
            // Parse the number into a System.Decimal.
            decimal result;

            if ((styles & NumberStyles.AllowHexSpecifier) == NumberStyles.None)
            {
                if (!decimal.TryParse(text, styles, Culture, out result))
                    throw new TypeConversionException($"Invalid {TargetType} value '{text}'");
            }
            else
            {
                if (!long.TryParse(text, styles, Culture, out var temp))
                    throw new TypeConversionException($"Invalid {TargetType} value '{text}'");
                result = temp;
            }

            try
            {
                // Convert the Decimal to the target type.
                return CreateNumber(result);
            }
            catch (Exception ex)
            {
                throw new TypeConversionException($"Invalid {TargetType} value '{text}'", ex);
            }
        }

        /// <summary>
        /// Parses a number from text.
        /// </summary>
        /// <param name="text">The text to convert to a number.</param>
        /// <returns>The parsed number.</returns>
        protected virtual object CreateNumber(string text)
        {
            return Parse(text, NumberStyles.Number);
        }

        /// <summary>
        /// Parses a number from a <see cref="decimal"/>.
        /// </summary>
        /// <param name="value">The <see cref="decimal"/> to convert to a number.</param>
        /// <returns>The parsed number.</returns>
        protected virtual object CreateNumber(decimal value)
        {
            return Convert.ChangeType(value, TargetType, Culture);
        }
    }
}
