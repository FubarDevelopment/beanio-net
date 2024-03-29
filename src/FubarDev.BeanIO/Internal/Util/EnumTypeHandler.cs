// <copyright file="EnumTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Config;
using BeanIO.Types;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// Default <see cref="Enum"/> type handler.
    /// </summary>
    internal class EnumTypeHandler : IConfigurableTypeHandler
    {
        private string? _enumFormat;

        private EnumFormatMode _enumFormatMode = EnumFormatMode.String;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTypeHandler"/> class.
        /// </summary>
        /// <param name="enumType">The type derived from <see cref="Enum"/>.</param>
        public EnumTypeHandler(Type enumType)
        {
            TargetType = enumType;
        }

        private enum EnumFormatMode
        {
            Format,
            String,
            LowerString,
            UpperString,
        }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record.</param>
        /// <returns>The parsed object.</returns>
        public object? Parse(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            try
            {
                return Enum.Parse(TargetType, text, true);
            }
            catch (Exception ex)
            {
                throw new TypeConversionException($"Invalid {TargetType.Name} enum value '{text}'", ex);
            }
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null.</param>
        /// <returns>The formatted field text, or <see langword="null" /> to indicate the value is not present.</returns>
        public string? Format(object? value)
        {
            if (value == null)
                return null;
            switch (_enumFormatMode)
            {
                case EnumFormatMode.LowerString:
                    return value.ToString()?.ToLowerInvariant();
                case EnumFormatMode.UpperString:
                    return value.ToString()?.ToUpperInvariant();
                case EnumFormatMode.String:
                    return value.ToString();
            }

            return Enum.Format(TargetType, value, _enumFormat ?? "g");
        }

        /// <summary>
        /// Configures this type handler.
        /// </summary>
        /// <param name="properties">The properties for customizing the instance.</param>
        public void Configure(Properties properties)
        {
            if (properties.TryGetValue("format", out var format))
            {
                if (string.IsNullOrEmpty(format) || format == "name")
                {
                    _enumFormatMode = EnumFormatMode.Format;
                    _enumFormat = "g";
                }
                else if (string.Equals(format, "toString", StringComparison.OrdinalIgnoreCase))
                {
                    _enumFormatMode = EnumFormatMode.String;
                    _enumFormat = null;
                }
                else if (string.Equals(format, "toLower", StringComparison.OrdinalIgnoreCase))
                {
                    _enumFormatMode = EnumFormatMode.LowerString;
                    _enumFormat = null;
                }
                else if (string.Equals(format, "toUpper", StringComparison.OrdinalIgnoreCase))
                {
                    _enumFormatMode = EnumFormatMode.UpperString;
                    _enumFormat = null;
                }
                else
                {
                    throw new BeanIOConfigurationException($"Invalid format '{format}', expected 'toString' or 'name' (default)");
                }
            }
        }
    }
}
