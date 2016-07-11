// <copyright file="CsvReader.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

using BeanIO.Internal.Util;
using BeanIO.Stream.Util;

namespace BeanIO.Stream.Csv
{
    /// <summary>
    /// A <see cref="CsvReader"/> is used to parse CSV formatted flat files into records of <see cref="System.String"/> arrays.
    /// </summary>
    /// <remarks>
    /// <para>The CSV format supported is defined by specification RFC 4180.
    /// By default, there is one exception: lines that span multiple records will
    /// throw an exception.  To allow quoted multi-line fields, simply set
    /// <see cref="P:CsvParserConfiguration.IsMultilineEnabled"/> to <code>true</code> when constructing the reader.</para>
    /// <para>
    /// The reader also supports the following customizations:
    /// <list type="bullet">
    /// <item>The default quotation mark character, '"', can be overridden.</item>
    /// <item>The default escape character, '"', can be overridden or disabled altogether.</item>
    /// <item>Whitespace can be allowed outside of quoted values.</item>
    /// <item>Quotation marks can be allowed in unquoted fields (as long as the quotation
    /// mark is not the first character in the field</item>
    /// </list>
    /// </para>
    /// <para>The reader will not recognize an escape character used outside of a quoted field.</para>
    /// </remarks>
    public class CsvReader : IRecordReader
    {
        private readonly string _delim;

        private readonly string _quote;

        private readonly string _endQuote;

        private readonly string _escapeChar;

        private readonly bool _multilineEnabled;

        private readonly bool _whitepsaceAllowed;

        private readonly bool _unquotedQuotesAllowed;

        private readonly CommentReader _commentReader;

        private readonly TextReader _in;

        private List<string> _fieldList = new List<string>();

        private int _lineNumber;

        private bool _skipLineFeed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// </summary>
        /// <param name="reader">the input stream to read from</param>
        public CsvReader(TextReader reader)
            : this(reader, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// </summary>
        /// <param name="reader">the input stream to read from</param>
        /// <param name="config">the reader configuration settings or <code>null</code> to accept defaults</param>
        public CsvReader(TextReader reader, CsvParserConfiguration config)
        {
            if (config == null)
                config = new CsvParserConfiguration();

            _in = reader;
            _delim = config.Delimiter.ToString();
            if (_delim == " ")
                throw new BeanIOConfigurationException(string.Format("The CSV field delimiter '{0}' is not supported", _delim));
            _quote = config.Quote.ToString();
            _endQuote = config.Quote.ToString();
            if (_quote == _delim)
                throw new BeanIOConfigurationException("The CSV field delimiter cannot match the character used for the quotation mark.");
            _multilineEnabled = config.IsMultilineEnabled;
            _whitepsaceAllowed = config.IsWhitespaceAllowed;
            _unquotedQuotesAllowed = config.UnquotedQuotesAllowed;
            _escapeChar = config.Escape == null ? null : config.Escape.ToString();
            if (_escapeChar != null && _escapeChar == _delim)
                throw new BeanIOConfigurationException("The CSV field delimiter cannot match the escape character.");
            if (config.IsCommentEnabled)
            {
                var markableReader = new MarkableTextReader(_in);
                _in = markableReader;
                _commentReader = new CommentReader(markableReader, config.Comments);
            }
        }

        /// <summary>
        /// Gets a single record from this input stream.
        /// </summary>
        /// <remarks>The type of object returned depends on the format of the stream.</remarks>
        /// <returns>
        /// The record value, or null if the end of the stream was reached.
        /// </returns>
        public int RecordLineNumber { get; private set; }

        /// <summary>
        /// Gets the unparsed record text of the last record read.
        /// </summary>
        /// <returns>
        /// The unparsed text of the last record read
        /// </returns>
        public string RecordText { get; private set; }

        /// <summary>
        /// Reads a single record from this input stream.
        /// </summary>
        /// <returns>
        /// The type of object returned depends on the format of the stream.
        /// </returns>
        /// <returns>The record value, or null if the end of the stream was reached.</returns>
        [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP2101:MethodMustNotContainMoreLinesThan", Justification = "Reviewed. Suppression is OK here.")]
        public object Read()
        {
            // _fieldList is set to null when the end of stream is reached
            if (_fieldList == null)
            {
                RecordText = null;
                RecordLineNumber = -1;
                return null;
            }

            ++_lineNumber;

            // skip comment lines
            if (_commentReader != null)
            {
                var lines = _commentReader.SkipComments(_skipLineFeed);
                if (lines > 0)
                {
                    if (_commentReader.IsEof)
                    {
                        _fieldList = null;
                        RecordText = null;
                        RecordLineNumber = -1;
                        return null;
                    }

                    _lineNumber += lines;
                    _skipLineFeed = _commentReader.SkipLineFeed;
                }
            }

            // the record line number is set to the first line of the record
            RecordLineNumber = _lineNumber;

            // clear the field list
            _fieldList.Clear();

            var state = 0;                      // current state
            var whitespace = 0;
            var escaped = false;                // last character read matched the escape char
            var eol = false;                    // end of record flag
            var text = new StringBuilder();     // holds the record text being read
            var field = new StringBuilder();    // holds the latest field value being read

            // parse an uncommented line
            int n;
            while (!eol && (n = _in.Read()) != -1)
            {
                var c = char.ConvertFromUtf32(n);

                // skip '\n' after a '\r'
                if (_skipLineFeed)
                {
                    _skipLineFeed = false;
                    if (c == "\n")
                    {
                        if (state == 1)
                        {
                            field.Append(c);
                            text.Append(c);
                        }

                        continue;
                    }
                }

                // append the raw record text
                if (c != "\n" && c != "\r")
                {
                    text.Append(c);
                }

                // handle escaped characters
                if (escaped)
                {
                    escaped = false;

                    // an escaped character can be used to escape itself or an end quote
                    if (c == _endQuote)
                    {
                        field.Append(c);
                        continue;
                    }

                    if (c == _escapeChar)
                    {
                        field.AppendFormat(_escapeChar);
                        continue;
                    }

                    if (_escapeChar == _endQuote)
                    {
                        _fieldList.Add(field.ToString());
                        field = new StringBuilder();
                        state = 10;
                    }
                }

                switch (state)
                {
                    case 0: // initial state (beginning of line, or next value)
                        if (c == _delim)
                        {
                            _fieldList.Add(whitespace.ToWhitespace());
                            whitespace = 0;
                        }
                        else if (c == _quote)
                        {
                            whitespace = 0;
                            state = 1; // look for trailing quote
                        }
                        else if (c == " ")
                        {
                            if (!_whitepsaceAllowed)
                            {
                                field.Append(c);
                                state = 2;
                            }
                            else
                            {
                                ++whitespace;
                            }
                        }
                        else if (c == "\r")
                        {
                            _fieldList.Add(string.Empty);
                            _skipLineFeed = true;
                            eol = true;
                        }
                        else if (c == "\n")
                        {
                            _fieldList.Add(string.Empty);
                            eol = true;
                        }
                        else
                        {
                            field.Append(whitespace.ToWhitespace());
                            whitespace = 0;
                            field.Append(c);
                            state = 2; // look for next delimiter
                        }

                        break;

                    case 1: // quoted field, look for trailing quote at end of field
                        if (_escapeChar != null && c == _escapeChar)
                        {
                            escaped = true;
                        }
                        else if (c == _endQuote)
                        {
                            _fieldList.Add(field.ToString());
                            field = new StringBuilder();
                            state = 10;
                        }
                        else if (c == "\r" || c == "\n")
                        {
                            if (!_multilineEnabled)
                                throw new RecordIOException(string.Format("Expected end quotation character '{0}' before end of line {1}", _endQuote, _lineNumber));
                            _skipLineFeed = c == "\r";
                            ++_lineNumber;
                            text.Append(c);
                            field.Append(c);
                        }
                        else
                        {
                            field.Append(c);
                        }

                        break;

                    case 2: // unquoted field, look for next delimiter
                        if (c == _delim)
                        {
                            _fieldList.Add(field.ToString());
                            field = new StringBuilder();
                            state = 0;
                        }
                        else if (c == _quote && !_unquotedQuotesAllowed)
                        {
                            Recover(text);
                            throw new RecordIOException(string.Format("Quotation character '{0}' must be quoted at line {1}", _quote, _lineNumber));
                        }
                        else if (c == "\n")
                        {
                            _fieldList.Add(field.ToString());
                            field = new StringBuilder();
                            eol = true;
                        }
                        else if (c == "\r")
                        {
                            _skipLineFeed = true;
                            _fieldList.Add(field.ToString());
                            field = new StringBuilder();
                            eol = true;
                        }
                        else
                        {
                            field.Append(c);
                        }

                        break;

                    case 10: // quoted field, after final quote read
                        if (c == " ")
                        {
                            if (!_whitepsaceAllowed)
                            {
                                Recover(text);
                                throw new RecordIOException(string.Format("Invalid whitespace found outside of quoted field at line {0}", _lineNumber));
                            }
                        }
                        else if (c == _delim)
                        {
                            state = 0;
                        }
                        else if (c == "\r")
                        {
                            _skipLineFeed = true;
                            eol = true;
                        }
                        else if (c == "\n")
                        {
                            eol = true;
                        }
                        else
                        {
                            Recover(text);
                            throw new RecordIOException(string.Format("Invalid character found outside of quoted field at line {0}", _lineNumber));
                        }

                        break;
                }
            }

            // if eol is true, we're done; if not, then the end of file was reached
            // and further validation is needed
            if (eol)
            {
                RecordText = text.ToString();
                return _fieldList.ToArray();
            }

            // handle escaped mode
            if (escaped)
            {
                if (_escapeChar == _endQuote)
                {
                    _fieldList.Add(field.ToString());
                    state = 10;
                }
            }

            // validate current state...
            switch (state)
            {
                case 0:
                    // do not create an empty field if we've reached the end of the file and no
                    // characters were read on the last line
                    if (whitespace > 0 || _fieldList.Count > 0)
                        _fieldList.Add(whitespace.ToWhitespace());
                    break;
                case 1:
                    _fieldList = null;
                    RecordText = null;
                    RecordLineNumber = -1;
                    throw new RecordIOException(string.Format("Expected end quote before end of line at line {0}", _lineNumber));
                case 2:
                    _fieldList.Add(field.ToString());
                    break;
                case 10:
                    break;
            }

            if (_fieldList.Count == 0)
            {
                _fieldList = null;
                RecordText = null;
                RecordLineNumber = -1;
                return null;
            }

            RecordText = text.ToString();
            var record = _fieldList.ToArray();
            _fieldList = null;
            return record;
        }

        /// <summary>
        /// Closes this input stream.
        /// </summary>
        public void Close()
        {
            _in.Dispose();
        }

        /// <summary>
        /// Advances the input stream to the end of the record so that subsequent reads
        /// might be possible.
        /// </summary>
        /// <param name="text">the current record text</param>
        private void Recover(StringBuilder text)
        {
            int n;
            while ((n = _in.Read()) != -1)
            {
                var c = char.ConvertFromUtf32(n);
                if (c == "\n")
                {
                    RecordText = text.ToString();
                    return;
                }

                if (c == "\r")
                {
                    _skipLineFeed = true;
                    RecordText = text.ToString();
                    return;
                }

                text.Append(c);
            }

            // end of file reached...
            RecordText = text.ToString();
            _fieldList = null;
        }
    }
}
