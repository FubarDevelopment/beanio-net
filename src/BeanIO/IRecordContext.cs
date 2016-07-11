// <copyright file="IRecordContext.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace BeanIO
{
    /// <summary>
    /// Provides information about a record parsed by a <see cref="IBeanReader"/> or <see cref="IUnmarshaller"/>.
    /// </summary>
    /// <remarks>
    /// Depending on the current state of the <see cref="IBeanReader"/> or <see cref="IUnmarshaller"/>, some
    /// information may not be available.
    /// </remarks>
    public interface IRecordContext
    {
        /// <summary>
        /// Gets the line number of this record, or 0 if the stream does not
        /// use new lines to terminate records.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets the raw text of the parsed record.
        /// </summary>
        /// <remarks>
        /// Record text is not supported by XML stream formats, and null is returned instead.
        /// </remarks>
        string RecordText { get; }

        /// <summary>
        /// Gets the name of the record from the stream configuration.
        /// </summary>
        /// <remarks>
        /// The record name may be null if was not determined before an exception was thrown.
        /// </remarks>
        string RecordName { get; }

        /// <summary>
        /// Gets a value indicating whether this record has any record or field level errors.
        /// </summary>
        bool HasErrors { get; }

        /// <summary>
        /// Gets a value indicating whether there are one or more record level errors.
        /// </summary>
        bool HasRecordErrors { get; }

        /// <summary>
        /// Gets a collection of record level error messages.
        /// </summary>
        IReadOnlyList<string> RecordErrors { get; }

        /// <summary>
        /// Gets a value indicating whether there are one or more field level errors.
        /// </summary>
        bool HasFieldErrors { get; }

        /// <summary>
        /// Returns the number of times the given field was present in the stream.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The number of times the field was present in the record.</returns>
        int GetFieldCount(string fieldName);

        /// <summary>
        /// Returns the raw text of a field found in this record.
        /// </summary>
        /// <remarks>
        /// <para>The field text may be null under the following circumstances:</para>
        /// <list type="bullet">
        /// <item>A record level exception was thrown before a field was parsed</item>
        /// <item><paramref name="fieldName" /> was not declared in the mapping file</item>
        /// <item>The field was not present in the record</item>
        /// </list>
        /// <para>If the field repeats in the stream, this method returns the field text for
        /// the first occurrence of the field.</para>
        /// </remarks>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The unparsed field text</returns>
        string GetFieldText(string fieldName);

        /// <summary>
        /// Returns the raw text of a field found in this record.
        /// </summary>
        /// <remarks>
        /// <para>The field text may be null under the following circumstances:</para>
        /// <list type="bullet">
        /// <item>A record level exception was thrown before a field was parsed</item>
        /// <item><paramref name="fieldName" /> was not declared in the mapping file</item>
        /// <item>The field was not present in the record</item>
        /// </list>
        /// </remarks>
        /// <param name="fieldName">The name of the field</param>
        /// <param name="index">The index of the field (beginning at 0), for repeating fields</param>
        /// <returns>The unparsed field text</returns>
        string GetFieldText(string fieldName, int index);

        /// <summary>
        /// Returns the field errors for a given field.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>
        /// The collection of field errors, or null if no errors were reported for the field.
        /// </returns>
        IReadOnlyList<string> GetFieldErrors(string fieldName);

        /// <summary>
        /// Gets a <see cref="ILookup{TKey,TItem}"/> of all field errors.
        /// </summary>
        /// <returns>All field errors</returns>
        ILookup<string, string> GetFieldErrors();
    }
}
