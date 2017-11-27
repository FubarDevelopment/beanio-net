// <copyright file="FixedLengthFieldPadding.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text;

namespace BeanIO.Internal.Parser.Format.FixedLength
{
    /// <summary>
    /// <see cref="FieldPadding"/> implementation for a fixed length field
    /// </summary>
    /// <remarks>
    /// Fixed length padding differs from other field padding in that a completely blank
    /// optional field (i.e. all spaces) is formatted as the empty string regardless of the filler
    /// character, thus allowing for optional numeric fields.
    /// </remarks>
    internal class FixedLengthFieldPadding : FieldPadding
    {
        /// <summary>
        /// Initializes padding settings
        /// </summary>
        /// <remarks>
        /// This method must be invoked before <see cref="FieldPadding.Pad"/> or <see cref="FieldPadding.Unpad"/> is called.
        /// </remarks>
        public override void Init()
        {
            base.Init();
            if (Length > 0)
            {
                var s = new StringBuilder(Length);
                s.Append(' ', Length);
                PaddedNull = s.ToString();
            }
        }

        /// <summary>
        /// Removes padding from the field text
        /// </summary>
        /// <param name="fieldText">the field text to remove padding</param>
        /// <returns>the unpadded field text</returns>
        public override string Unpad(string fieldText)
        {
            // return empty string if the field is all spaces, to allow for optional
            // zero padded fields
            if (IsOptional && string.IsNullOrWhiteSpace(fieldText))
                return string.Empty;
            return base.Unpad(fieldText);
        }
    }
}
