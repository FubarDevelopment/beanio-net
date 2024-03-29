// <copyright file="ErrorContext.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace BeanIO.Internal.Parser
{
    internal class ErrorContext : IRecordContext
    {
        private readonly List<string> _recordErrors = new List<string>();

        private readonly Dictionary<string, string?> _fieldTexts = new Dictionary<string, string?>();

        private readonly Dictionary<string, Counter> _fieldCounters = new Dictionary<string, Counter>();

        private readonly Dictionary<string, List<string>> _fieldErrors = new Dictionary<string, List<string>>();

        private int _lineNumber;

        /// <summary>
        /// Gets or sets the line number of this record, or 0 if the stream does not
        /// use new lines to terminate records.
        /// </summary>
        public int LineNumber
        {
            get
            {
                return _lineNumber;
            }
            set
            {
                if (value > 0)
                    _lineNumber = value;
            }
        }

        /// <summary>
        /// Gets the starting line number of the last record read from the record reader.
        /// </summary>
        public virtual int RecordLineNumber => LineNumber;

        /// <summary>
        /// Gets or sets the raw text of the parsed record.
        /// </summary>
        /// <remarks>
        /// Record text is not supported by XML stream formats, and null is returned instead.
        /// </remarks>
        public string? RecordText { get; set; }

        /// <summary>
        /// Gets or sets the name of the record from the stream configuration.
        /// </summary>
        /// <remarks>
        /// The record name may be null if was not determined before an exception was thrown.
        /// </remarks>
        public string? RecordName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this record has any record or field level errors.
        /// </summary>
        public bool HasErrors => HasRecordErrors || HasFieldErrors;

        /// <summary>
        /// Gets a value indicating whether there are one or more record level errors.
        /// </summary>
        public bool HasRecordErrors => _recordErrors.Count != 0;

        /// <summary>
        /// Gets a collection of record level error messages.
        /// </summary>
        public IReadOnlyList<string> RecordErrors => _recordErrors;

        /// <summary>
        /// Gets a value indicating whether there are one or more field level errors.
        /// </summary>
        public bool HasFieldErrors => _fieldErrors.Count != 0;

        /// <summary>
        /// Returns the number of times the given field was present in the stream.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The number of times the field was present in the record.</returns>
        public int GetFieldCount(string fieldName)
        {
            if (_fieldCounters.TryGetValue(fieldName, out var counter))
                return counter.Count;
            return _fieldTexts.ContainsKey(fieldName) ? 1 : 0;
        }

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
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The unparsed field text.</returns>
        public string? GetFieldText(string fieldName)
        {
            return GetFieldText(fieldName, 0);
        }

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
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="index">The index of the field (beginning at 0), for repeating fields.</param>
        /// <returns>The unparsed field text.</returns>
        public string? GetFieldText(string fieldName, int index)
        {
            var key = !_fieldCounters.ContainsKey(fieldName) && index == 0
                          ? fieldName
                          : $"{index}:{fieldName}";
            if (_fieldTexts.TryGetValue(key, out var fieldText))
                return fieldText;
            return null;
        }

        /// <summary>
        /// Returns the field errors for a given field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>
        /// The collection of field errors, or null if no errors were reported for the field.
        /// .</returns>
        public IReadOnlyList<string> GetFieldErrors(string fieldName)
        {
            if (_fieldErrors.TryGetValue(fieldName, out var errors))
                return errors;
            return Array.Empty<string>();
        }

        /// <summary>
        /// Gets a <see cref="ILookup{TKey,TItem}"/> of all field errors.
        /// </summary>
        /// <returns>All field errors.</returns>
        public ILookup<string, string> GetFieldErrors()
        {
            return _fieldErrors
                .SelectMany(x => x.Value.Select(y => new { x.Key, Value = y }))
                .ToLookup(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Clears this context.
        /// </summary>
        public void Clear()
        {
            _lineNumber = 0;
            RecordName = null;
            RecordText = null;
            _fieldTexts.Clear();
            _fieldErrors.Clear();
            _fieldCounters.Clear();
            _recordErrors.Clear();
        }

        public void AddFieldError(string fieldName, string message)
        {
            if (!_fieldErrors.TryGetValue(fieldName, out var errors))
                _fieldErrors.Add(fieldName, errors = new List<string>());
            errors.Add(message);
        }

        public void AddRecordError(string message)
        {
            _recordErrors.Add(message);
        }

        public void SetFieldText(string fieldName, string? text, bool isRepeating)
        {
            if (isRepeating)
            {
                if (!_fieldCounters.TryGetValue(fieldName, out var counter))
                    _fieldCounters.Add(fieldName, counter = new Counter());

                _fieldTexts.Add($"{counter.Count}:{fieldName}", text);

                counter.Increment();
            }
            else
            {
                _fieldTexts[fieldName] = text;
            }
        }

        private class Counter
        {
            public int Count { get; private set; }

            public void Increment()
            {
                ++Count;
            }
        }
    }
}
