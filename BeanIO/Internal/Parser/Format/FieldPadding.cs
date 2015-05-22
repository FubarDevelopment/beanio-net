using System;
using System.Reflection;
using System.Text;

using BeanIO.Builder;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Format
{
    public class FieldPadding
    {
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
        public Type PropertyType { get; set; }

        public void Init()
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
                else if (PropertyType.GetTypeInfo().IsPrimitive)
                {
                    if (char.IsDigit(Filler))
                    {
                        DefaultText = Filler.ToString();
                    }
                }
            }
        }

        public string Pad(string text)
        {
            int currentLength;
            if (string.IsNullOrEmpty(text))
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

        public string Unpad(string fieldText)
        {
            if (Justify == Align.Left)
            {
                fieldText = fieldText.TrimEnd(Filler);
            }
            else
            {
                fieldText = fieldText.TrimStart(Filler);
            }

            if (string.IsNullOrEmpty(fieldText))
                return DefaultText;

            return fieldText;
        }
    }
}
