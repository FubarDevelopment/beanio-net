// <copyright file="FixedLengthRecordFormat.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Format.FixedLength
{
    /// <summary>
    ///  A <see cref="IRecordFormat"/> implementation for a fixed length formatted record.
    /// </summary>
    /// <remarks>
    /// A fixed length record may be configured to validate record length by
    /// setting a minimum and maximum length.
    /// </remarks>
    internal class FixedLengthRecordFormat : IRecordFormat
    {
        public FixedLengthRecordFormat()
        {
            MaxLength = MaxMatchLength = int.MaxValue;
        }

        /// <summary>
        /// Gets or sets the minimum number of fields in the record.
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of fields in the record.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum record length for identifying a record.
        /// </summary>
        public int MinMatchLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum record length for identifying a record.
        /// </summary>
        public int? MaxMatchLength { get; set; }

        /// <summary>
        /// Returns whether the record meets configured matching criteria during unmarshalling.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/>.</param>
        /// <returns>true if the record meets all matching criteria, false otherwise.</returns>
        public bool Matches(UnmarshallingContext context)
        {
            var length = ((FixedLengthUnmarshallingContext)context).RecordLength;
            return length >= MinMatchLength && (MaxMatchLength == null || length <= MaxMatchLength);
        }

        /// <summary>
        /// Validates a record during unmarshalling.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/>.</param>
        public void Validate(UnmarshallingContext context)
        {
            var length = ((FixedLengthUnmarshallingContext)context).RecordLength;
            if (length < MinLength)
                context.AddRecordError("minLength", MinLength, MaxLength ?? int.MaxValue);
            if (MaxLength != null && length > MaxLength)
                context.AddRecordError("maxLength", MinLength, MaxLength);
        }

        public override string ToString()
        {
            var s = new StringBuilder()
                .AppendFormat(
                    "{0}[recordLength={1}, ridLength={2}]",
                    GetType().Name,
                    DebugUtil.FormatRange(MinLength, MaxLength),
                    DebugUtil.FormatRange(MinMatchLength, MaxMatchLength));
            return s.ToString();
        }
    }
}
