// <copyright file="FieldPadding.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Text;

using BeanIO.Builder;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Format
{
    /// <summary>
    /// Provides field padding functionality. By default, padded fields are
    /// left justified and padded using a space.
    /// </summary>
    /// <remarks>
    /// <para>The method <see cref="Init"/> must be called after all properties are set.</para>
    /// <para>If <see cref="IsOptional"/> is set to true, the field text may be padded with spaces
    /// regardless of the configured <see cref="Filler"/> when a value does not exist.</para>
    /// <para>Once configured, a <see cref="FieldPadding"/> object is thread-safe.</para>
    /// </remarks>
    internal class FieldPadding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldPadding"/> class.
        /// </summary>
        public FieldPadding()
        {
            Filler = ' ';
            Justify = Align.Left;
            DefaultText = PaddedNull = string.Empty;
        }

        /// <summary>
        /// Gets or sets the character used to pad field text.
        /// </summary>
        public char Filler { get; set; }

        /// <summary>
        /// Gets or sets the justification of the field text within its padding.
        /// </summary>
        public Align Justify { get; set; }

        /// <summary>
        /// Gets or sets the padded length of the field.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the default value returned when the result of an <see cref="Unpad"/> would result in an empty string.
        /// </summary>
        public string DefaultText { get; set; }

        /// <summary>
        /// Gets or sets the padded field text for a null value.
        /// </summary>
        public string PaddedNull { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is optional.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Gets or sets the property type of the field.
        /// </summary>
        public Type? PropertyType { get; set; }

        /// <summary>
        /// Initializes padding settings.
        /// </summary>
        /// <remarks>
        /// This method must be invoked before <see cref="Pad"/> or <see cref="Unpad"/> is called.
        /// </remarks>
        public virtual void Init()
        {
            if (PropertyType == null)
            {
                DefaultText = string.Empty;
                IsOptional = false;
            }
            else
            {
                if (PropertyType.IsInstanceOf(typeof(char)))
                {
                    DefaultText = Filler.ToString();
                    IsOptional = false;
                }
                else if (PropertyType.IsPrimitive)
                {
                    if (char.IsDigit(Filler))
                    {
                        DefaultText = Filler.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Formats field text.
        /// </summary>
        /// <remarks>If the length of <paramref name="text"/> exceeds the padding length,
        /// the text will be truncated, otherwise it will be padded with <see cref="Filler"/>.
        /// </remarks>
        /// <param name="text">the field text to format.</param>
        /// <returns>the formatted field text.</returns>
        public virtual string Pad(string? text)
        {
            int currentLength;
            if (text == null)
            {
                if (IsOptional)
                    return PaddedNull;
                text = string.Empty;
                currentLength = 0;
            }
            else if (Length <= 0)
            {
                return text;
            }
            else
            {
                currentLength = text.Length;
                if (currentLength == Length)
                    return text;
                if (currentLength > Length)
                    return text.Substring(0, Length);
            }

            var remaining = Length - currentLength;
            var s = new StringBuilder(Length);
            if (Justify == Align.Left)
            {
                s.Append(text)
                 .Append(Filler, remaining);
            }
            else
            {
                s.Append(Filler, remaining)
                 .Append(text);
            }

            return s.ToString();
        }

        /// <summary>
        /// Removes padding from the field text.
        /// </summary>
        /// <param name="fieldText">the field text to remove padding.</param>
        /// <returns>the unpadded field text.</returns>
        public virtual string Unpad(string fieldText)
        {
            fieldText = Justify == Align.Left ? fieldText.TrimEnd(Filler) : fieldText.TrimStart(Filler);

            if (string.IsNullOrEmpty(fieldText))
                return DefaultText;

            return fieldText;
        }
    }
}
