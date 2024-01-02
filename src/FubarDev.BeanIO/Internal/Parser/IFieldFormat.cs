// <copyright file="IFieldFormat.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="IFieldFormat"/> provides format specific processing for a <see cref="Field"/> parser.
    /// </summary>
    internal interface IFieldFormat
    {
        /// <summary>
        /// Gets the size of the field.
        /// </summary>
        /// <remarks>
        /// Fixed length formats should return the field length, while other formats should simply return 1.
        /// </remarks>
        int Size { get; }

        /// <summary>
        /// Gets a value indicating whether this field is nillable.
        /// </summary>
        bool IsNillable { get; }

        /// <summary>
        /// Gets a value indicating whether this field is optionally present in the record.
        /// </summary>
        /// <remarks>
        /// TODO: rename isLazy to something better??
        /// </remarks>
        bool IsLazy { get; }

        /// <summary>
        /// Extracts the field text from a record.
        /// </summary>
        /// <remarks>
        /// <para>May return <see cref="F:Value.Invalid"/> if the field is invalid, or <see cref="F:Value.Nil"/>
        /// if the field is explicitly set to nil or null such as in an XML formatted
        /// stream.</para>
        /// <para>Implementations should also remove any field padding before returning the text.</para>
        /// </remarks>
        /// <param name="context">the <see cref="UnmarshallingContext"/> holding the record.</param>
        /// <param name="reportErrors">report the errors?.</param>
        /// <returns>the field text or null if the field was not present in the record.</returns>
        string? Extract(UnmarshallingContext context, bool reportErrors);

        /// <summary>
        /// Inserts a value into a record.
        /// </summary>
        /// <remarks>
        /// <para>This method is called before type conversion.</para>
        /// <para>If the method returns false, type conversion is invoked and <see cref="InsertField"/>
        /// is called. If the method returns true, <see cref="InsertField"/>
        /// is not invoked.</para>
        /// </remarks>
        /// <param name="context">the <see cref="MarshallingContext"/>.</param>
        /// <param name="value">the value to insert into the record.</param>
        /// <returns>true if type conversion is required and <see cref="InsertField"/> must be invoked, false otherwise.</returns>
        bool InsertValue(MarshallingContext context, object? value);

        /// <summary>
        /// Inserts field text into a record.
        /// </summary>
        /// <param name="context">the <see cref="MarshallingContext"/> holding the record.</param>
        /// <param name="text">the field text to insert into the record.</param>
        void InsertField(MarshallingContext context, string? text);
    }
}
