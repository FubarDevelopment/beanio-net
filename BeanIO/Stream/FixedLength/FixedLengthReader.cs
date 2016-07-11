// <copyright file="FixedLengthReader.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using System.Text;

using BeanIO.Internal.Util;
using BeanIO.Stream.Util;

namespace BeanIO.Stream.FixedLength
{
    /// <summary>
    /// A <see cref="FixedLengthReader"/> is used to read records from a fixed length file or input stream.
    /// </summary>
    /// <remarks>
    /// <para>A fixed length record is represented using the <see cref="String"/> class.
    /// Records must be terminated by a single configurable character, or by
    /// default, any of the following: line feed (LF), carriage return (CR), or
    /// CRLF combination.</para>
    /// <para>
    /// If a record may span multiple lines, a single line continuation
    /// character may be configured.  The line continuation character must immediately
    /// precede the record termination character.  Note that line continuation characters
    /// are not included in the record text.
    /// </para>
    /// </remarks>
    public class FixedLengthReader : IRecordReader
    {
        private readonly char _lineContinuationChar = '\\';

        private readonly bool _multilineEnabled;

        private readonly char? _recordTerminator;

        private readonly CommentReader _commentReader;

        private readonly TextReader _in;

        private string _recordText;

        private int _recordLineNumber;

        private int _lineNumber;

        private bool _skipLineFeed;

        private bool _eof;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedLengthReader"/> class.
        /// </summary>
        /// <param name="reader">the input stream to read from</param>
        public FixedLengthReader(TextReader reader)
            : this(reader, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedLengthReader"/> class.
        /// </summary>
        /// <param name="reader">the input stream to read from</param>
        /// <param name="config">the reader configuration settings or null to accept defaults</param>
        public FixedLengthReader(TextReader reader, FixedLengthParserConfiguration config)
        {
            if (config == null)
                config = new FixedLengthParserConfiguration();

            _in = reader;

            if (config.RecordTerminator != null)
            {
                var s = config.RecordTerminator;
                if (string.Equals("\r\n", s, StringComparison.Ordinal))
                {
                    // use default
                }
                else if (s.Length == 1)
                {
                    _recordTerminator = s[0];
                }
                else if (s.Length > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(config), "Record terminator must be a single character");
                }
            }

            if (config.LineContinuationCharacter == null)
            {
                _multilineEnabled = false;
            }
            else
            {
                _multilineEnabled = true;
                _lineContinuationChar = config.LineContinuationCharacter.Value;

                if (_recordTerminator != null && _lineContinuationChar == _recordTerminator)
                    throw new ArgumentException("The line continuation character and record terminator cannot match.");
            }

            if (config.IsCommentEnabled)
            {
                var markableReader = (reader as MarkableTextReader) ?? new MarkableTextReader(reader);
                _in = markableReader;
                _commentReader = new CommentReader(markableReader, config.Comments, _recordTerminator);
            }
        }

        /// <summary>
        /// Gets a single record from this input stream.
        /// </summary>
        /// <remarks>The type of object returned depends on the format of the stream.</remarks>
        /// <returns>
        /// The record value, or null if the end of the stream was reached.
        /// </returns>
        public int RecordLineNumber
        {
            get
            {
                if (_recordLineNumber < 0)
                    return _recordLineNumber;
                return _recordTerminator == null ? _recordLineNumber : 0;
            }
        }

        /// <summary>
        /// Gets the unparsed record text of the last record read.
        /// </summary>
        /// <returns>
        /// The unparsed text of the last record read
        /// </returns>
        public string RecordText => _recordText;

        /// <summary>
        /// Reads a single record from this input stream.
        /// </summary>
        /// <returns>
        /// The type of object returned depends on the format of the stream.
        /// </returns>
        /// <returns>The record value, or null if the end of the stream was reached.</returns>
        public object Read()
        {
            if (_eof)
            {
                _recordText = null;
                _recordLineNumber = -1;
                return null;
            }

            ++_lineNumber;

            // skip commented lines
            if (_commentReader != null)
            {
                var lines = _commentReader.SkipComments(_skipLineFeed);
                if (lines > 0)
                {
                    if (_commentReader.IsEof)
                    {
                        _eof = true;
                        _recordText = null;
                        _recordLineNumber = -1;
                        return null;
                    }

                    _lineNumber += lines;
                    _skipLineFeed = _commentReader.SkipLineFeed;
                }
            }

            var lineOffset = 0;
            var continued = false; // line continuation
            var eol = false; // end of record flag
            var text = new StringBuilder();
            var record = new StringBuilder();

            int n;
            while (!eol && (n = _in.Read()) != -1)
            {
                var c = char.ConvertFromUtf32(n)[0];

                // skip '\n' after a '\r'
                if (_skipLineFeed)
                {
                    _skipLineFeed = false;
                    if (c == '\n')
                        continue;
                }

                // handle line continuation
                if (continued)
                {
                    continued = false;
                    text.Append(c);
                    if (IsEndOfRecord(c))
                    {
                        ++_lineNumber;
                        ++lineOffset;
                        continue;
                    }

                    record.Append(_lineContinuationChar);
                }

                if (_multilineEnabled && c == _lineContinuationChar)
                {
                    continued = true;
                }
                else if (IsEndOfRecord(c))
                {
                    eol = true;
                }
                else
                {
                    text.Append(c);
                    record.Append(c);
                }
            }

            // update the record line number
            _recordLineNumber = _lineNumber - lineOffset;
            _recordText = text.ToString();

            // if eol is true, we're done; if not, then the end of file was reached
            // and further validation is needed
            if (eol)
            {
                return record.ToString();
            }

            _eof = true;

            if (continued)
            {
                _recordText = null;
                _recordLineNumber = -1;
                throw new RecordIOException(string.Format("Unexpected end of stream after line continuation at line {0}", _lineNumber));
            }

            if (_recordText.Length == 0)
            {
                _recordText = null;
                _recordLineNumber = -1;
                return null;
            }

            return record.ToString();
        }

        /// <summary>
        /// Closes this input stream.
        /// </summary>
        public void Close()
        {
            _in.Dispose();
        }

        /// <summary>
        /// Returns true if the given character matches the record separator
        /// </summary>
        /// <remarks>This method also updates the internal <see cref="F:_skipLF"/> flag.</remarks>
        /// <param name="ch">the character to test</param>
        /// <returns>true if the character signifies the end of the record</returns>
        private bool IsEndOfRecord(char ch)
        {
            if (_recordTerminator != null)
                return ch == _recordTerminator;

            if (ch == '\r')
            {
                _skipLineFeed = true;
                return true;
            }

            if (ch == '\n')
                return true;

            return false;
        }
    }
}
