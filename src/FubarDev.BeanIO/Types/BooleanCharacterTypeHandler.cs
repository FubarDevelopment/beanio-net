// <copyright file="BooleanCharacterTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Types
{
    /// <summary>
    /// Type handler for <see cref="bool"/> which uses a character representation.
    /// </summary>
    public class BooleanCharacterTypeHandler : ITypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanCharacterTypeHandler"/> class.
        /// </summary>
        public BooleanCharacterTypeHandler()
        {
            TrueValue = 'Y';
            FalseValue = 'N';
        }

        /// <summary>
        /// Gets or sets the character to be used for <c>true</c>.
        /// </summary>
        public char TrueValue { get; set; }

        /// <summary>
        /// Gets or sets the character to be used for <see langword="false" />.
        /// </summary>
        public char? FalseValue { get; set; }

        /// <summary>
        /// Gets or sets the character to be used for <see langword="null" />.
        /// </summary>
        public char? NullValue { get; set; }

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

            if (text!.Length != 1)
                throw new FormatException($"Invalid value '{text}' (too long)");

            var ch = text[0];
            if (ch == TrueValue)
                return true;
            if (FalseValue.HasValue && FalseValue == ch)
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
                return $"{NullValue}";
            var boolValue = (bool)value;
            return $"{(boolValue ? TrueValue : FalseValue)}";
        }
    }
}
