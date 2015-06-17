﻿using System;
using System.Globalization;

namespace BeanIO.Types
{
    /// <summary>
    /// A type handler implementation for the <see cref="System.Single"/> class.
    /// </summary>
    public class SingleTypeHandler : NumberTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleTypeHandler"/> class.
        /// </summary>
        public SingleTypeHandler()
            : base(typeof(float))
        {
        }

        /// <summary>
        /// Parses a string to a number by converting the text first to a decimal number and than
        /// to the target type.
        /// </summary>
        /// <param name="text">The text to convert</param>
        /// <param name="styles">The number styles to use</param>
        /// <returns>The parsed number</returns>
        protected override object Parse(string text, NumberStyles styles)
        {
            float result;
            if (!float.TryParse(text, styles, Culture, out result))
                throw new TypeConversionException(string.Format("Invalid {0} value '{1}'", TargetType, text));
            return result;
        }

        /// <summary>
        /// Parses a number from text.
        /// </summary>
        /// <param name="text">The text to convert to a number</param>
        /// <returns>The parsed number</returns>
        protected override object CreateNumber(string text)
        {
            return Parse(text, NumberStyles.Float);
        }
    }
}
