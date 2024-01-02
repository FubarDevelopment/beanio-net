// <copyright file="DelimitedUnmarshallingContext.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser.Format.Delimited
{
    /// <summary>
    /// An <see cref="UnmarshallingContext"/> for a delimited record.
    /// </summary>
    /// <remarks>
    /// The record value type for a delimited record is a <see cref="string" /> array.
    /// </remarks>
    internal class DelimitedUnmarshallingContext : UnmarshallingContext
    {
        private string?[]? _fields;

        /// <summary>
        /// Gets the number of fields read from the input stream.
        /// </summary>
        public int FieldCount => _fields?.Length ?? 0;

        /// <summary>
        /// Returns the field text at the given position in the record.
        /// </summary>
        /// <param name="fieldName">the field name.</param>
        /// <param name="position">the position of the field within the record.</param>
        /// <param name="until">the maximum position of the field as an offset
        /// of the field count, for example -2 to indicate the any position
        /// except the last two fields in the record.</param>
        /// <returns>the field text.</returns>
        public string? GetFieldText(string fieldName, int position, int until)
        {
            if (position < 0)
            {
                position = FieldCount + position;
                position = GetAdjustedFieldPosition(position);
                if (position < 0)
                    return null;
            }
            else
            {
                until = FieldCount + until;
                position = GetAdjustedFieldPosition(position);
                if (position >= until)
                    return null;
            }

            if (_fields == null)
            {
                throw new InvalidOperationException("Fields aren't set");
            }

            var text = _fields[position];
            SetFieldText(fieldName, text);
            return text;
        }

        /// <summary>
        /// Sets the value of the record returned from the <see cref="IRecordReader"/>.
        /// </summary>
        /// <param name="value">the record value read by a <see cref="IRecordReader"/>.</param>
        public override void SetRecordValue(object? value)
        {
            _fields = (string?[]?)value;
        }

        /// <summary>
        /// Converts a <see cref="string"/>[] to a record value.
        /// </summary>
        /// <param name="array">the <see cref="string"/>[] to convert.</param>
        /// <returns>the record value, or null if not supported.</returns>
        public override object ToRecordValue(string?[] array)
        {
            return array;
        }

        /// <summary>
        /// Converts a <see cref="List{T}"/> to a record value.
        /// </summary>
        /// <param name="list">the <see cref="List{T}"/> to convert.</param>
        /// <returns>the record value, or null if not supported.</returns>
        public override object ToRecordValue(IList<string?> list)
        {
            return list.ToArray();
        }
    }
}
