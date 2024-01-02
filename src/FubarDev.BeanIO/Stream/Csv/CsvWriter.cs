// <copyright file="CsvWriter.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

namespace BeanIO.Stream.Csv
{
    /// <summary>
    /// A <see cref="CsvWriter"/> is used to format and write records, of <see cref="string"/> arrays,
    /// to a CSV output stream.
    /// </summary>
    /// <remarks>
    /// <para>Using default settings, the CSV format supported is defined by specification RFC 4180.</para>
    /// <para>
    /// The writer also supports the following customizations:
    /// <list type="bullet">
    /// <item>The default field delimiter, ',', may be overridden.</item>
    /// <item>The default quotation mark, '"', may be overridden.</item>
    /// <item>The default escape character, '"', may be overridden.</item>
    /// <item>The writer can be configured to quote every field. Otherwise a
    /// field is only quoted if it contains a quotation mark, delimiter,
    /// line feed or carriage return.</item>
    /// </list>
    /// </para>
    /// </remarks>
    public class CsvWriter : IRecordWriter
    {
        private readonly char _delim;

        private readonly char _quote;

        private readonly char _endQuote;

        private readonly char _escapeChar;

        private readonly string _lineSeparator;

        private readonly bool _alwaysQuote;

        private readonly TextWriter _out;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvWriter"/> class.
        /// </summary>
        /// <param name="textWriter">the output stream to write to.</param>
        public CsvWriter(TextWriter textWriter)
            : this(textWriter, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvWriter"/> class.
        /// </summary>
        /// <param name="textWriter">the output stream to write to.</param>
        /// <param name="config">the <see cref="CsvParserConfiguration"/>.</param>
        public CsvWriter(TextWriter textWriter, CsvParserConfiguration? config)
        {
            config ??= new CsvParserConfiguration();

            _out = textWriter;
            _delim = config.Delimiter;
            _quote = config.Quote;
            _endQuote = config.Quote;
            _alwaysQuote = config.AlwaysQuote;
            _escapeChar = config.Escape.GetValueOrDefault('"');
            _lineSeparator = config.RecordTerminator ?? textWriter.NewLine;
        }

        /// <summary>
        /// Gets the last line number written to the output stream.
        /// </summary>
        public int LineNumber { get; private set; }

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
        /// Writes a record object to this output stream.
        /// </summary>
        /// <param name="record">Record the record object to write.</param>
        public void Write(string[] record)
        {
            LineNumber += 1;

            var pos = 0;
            foreach (var field in record)
            {
                if (pos++ > 0)
                    _out.Write(_delim);

                var skipLineFeed = false;
                var cs = field.ToCharArray();

                var quoted = _alwaysQuote || cs.MustQuote(_delim, _quote);
                if (quoted)
                    _out.Write(_quote);

                foreach (var c in cs)
                {
                    if (c == _endQuote || c == _escapeChar)
                    {
                        _out.Write(_escapeChar);
                    }
                    else if (c == '\r')
                    {
                        skipLineFeed = true;
                        LineNumber += 1;
                    }
                    else if (c == '\n')
                    {
                        if (skipLineFeed)
                        {
                            skipLineFeed = false;
                        }
                        else
                        {
                            LineNumber += 1;
                        }
                    }
                    else
                    {
                        skipLineFeed = false;
                    }

                    _out.Write(c);
                }

                if (quoted)
                    _out.Write(_endQuote);
            }

            _out.Write(_lineSeparator);
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
    }
}
