// <copyright file="IFlatFieldFormat.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser.Format.Flat
{
    /// <summary>
    /// A <see cref="IFlatFieldFormat"/> is a <see cref="IFieldFormat"/> for flat stream formats
    /// (i.e. CSV, delimited and fixed length).
    /// </summary>
    internal interface IFlatFieldFormat : IFieldFormat
    {
        /// <summary>
        /// Gets the field position.
        /// </summary>
        /// <remarks>
        /// <para>In a delimited/CSV stream format, the position is the index of the field in the
        /// record starting at 0. For example, the position of field2 in the following
        /// comma delimited record is 1:</para>
        /// <example>field1,field2,field3</example>
        /// <para>In a fixed length stream format, the position is the index of the first character
        /// of the field in the record, also starting at 0.  For example, the position of field2
        /// in the following record is 6:</para>
        /// <example>field1field2field3</example>
        /// </remarks>
        int Position { get; }
    }
}
