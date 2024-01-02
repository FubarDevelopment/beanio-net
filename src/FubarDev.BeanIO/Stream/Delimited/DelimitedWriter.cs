// <copyright file="DelimitedWriter.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

namespace BeanIO.Stream.Delimited
{
    /// <summary>
    /// A <see cref="DelimitedWriter"/> is used to write records to delimited flat files.
    /// Each record must be a String array of fields.
    /// </summary>
    /// <remarks>
    /// <para>By default, fields are delimited by the tab character, but any other single
    /// character may be configured instead.</para>
    /// <para>If an escape character is configured, any field containing the delimiter
    /// will be escaped by placing the escape character immediately before the
    /// delimiter.  For example, if the record "Field1,2", "Field3" is written
    /// using a comma delimiter and backslash escape character, the following text
    /// will be written to the output stream:
    /// <c>
    /// Field1\,2,Field3
    /// </c>
    /// Note that no validation is performed when a record is written, so if an escape character
    /// is not configured and a field contains a delimiting character, the generated
    /// output may be invalid.</para>
    /// </remarks>
    public class DelimitedWriter : IRecordWriter
    {
        private readonly char _delim;

        private readonly char? _escapeChar;

        private readonly string _recordTerminator;

        private readonly TextWriter _out;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedWriter"/> class.
        /// </summary>
        /// <param name="textWriter">the output stream to write to.</param>
        public DelimitedWriter(TextWriter textWriter)
            : this(textWriter, '\t')
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedWriter"/> class.
        /// </summary>
        /// <param name="textWriter">the output stream to write to.</param>
        /// <param name="delimiter">the field delimiting character.</param>
        public DelimitedWriter(TextWriter textWriter, char delimiter)
            : this(textWriter, new DelimitedParserConfiguration(delimiter))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedWriter"/> class.
        /// </summary>
        /// <param name="textWriter">the output stream to write to.</param>
        /// <param name="config">the delimited parser configuration.</param>
        public DelimitedWriter(TextWriter textWriter, DelimitedParserConfiguration config)
        {
            _out = textWriter;
            _delim = config.Delimiter;
            _escapeChar = config.Escape;
            if (_escapeChar != null && _escapeChar == _delim)
                throw new BeanIOConfigurationException("Delimiter cannot match the escape character");
            _recordTerminator = config.RecordTerminator ?? textWriter.NewLine;
        }

        /// <summary>
        /// Writes a record object to this output stream.
        /// </summary>
        /// <param name="record">Record the record object to write.</param>
        public void Write(object? record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            Write((string[])record);
        }

        /// <summary>
        /// Flushes the output stream.
        /// </summary>
        public void Flush()
        {
            _out.Flush();
        }

        /// <summary>
        /// Closes the output stream.
        /// </summary>
        public void Close()
        {
            _out.Dispose();
        }

        /// <summary>
        /// Writes a record object to this output stream.
        /// </summary>
        /// <param name="record">Record the record object to write.</param>
        public void Write(string[] record)
        {
            if (_escapeChar != null)
            {
                var pos = 0;
                foreach (var field in record)
                {
                    if (pos++ > 0)
                        _out.Write(_delim);

                    var cs = field.ToCharArray();
                    for (int i = 0, j = cs.Length; i < j; ++i)
                    {
                        var c = cs[i];
                        if (c == _delim || c == _escapeChar)
                            _out.Write(_escapeChar);
                        _out.Write(c);
                    }
                }
            }
            else
            {
                var pos = 0;
                foreach (var field in record)
                {
                    if (pos++ > 0)
                        _out.Write(_delim);
                    _out.Write(field);
                }
            }

            _out.Write(_recordTerminator);
        }
    }
}
