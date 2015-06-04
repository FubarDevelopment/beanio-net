using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using BeanIO.Internal.Util;
using BeanIO.Stream.Util;

namespace BeanIO.Stream.Delimited
{
    /// <summary>
    /// A <see cref="DelimitedReader"/> is used to parse delimited flat files into
    /// records of <code>String</code> arrays.
    /// </summary>
    /// <remarks>
    /// <para>Records must be terminated by a single configurable character,
    /// or by default, any of the following: line feed (LF), carriage
    /// return (CR), or CRLF combination.  And fields that make up a record
    /// must be delimited by a single configurable character.</para>
    /// <para>If an escape character is configured, the delimiting character can be
    /// escaped in a field by placing the escape character immediately before
    /// the delimiter.  The escape character may also be used to escape itself.
    /// For example, using a comma delimiter and backslash escape:
    /// <code>
    /// Field1,Field2\,Field3,Field\\4,Field\5
    /// </code>
    /// The record would be parsed as "Field1", "Field2,Field3", "Field\4", "Field\5"</para>
    /// <para>Additionally, if a record may span multiple lines, a single line continuation
    /// character can be configured.  The line continuation character must immediately
    /// precede the record termination character. For example, using a comma delimiter
    /// and backslash line continuation character:
    /// <code>
    /// Field1,Field2\
    /// Field3
    /// </code>
    /// The 2 lines would be parsed as a single record with values "Field1", "Field2", "Field3".</para>
    /// <para>The same character can be used for line continuation and escaping, but neither
    /// can match the delimiter.</para>
    /// </remarks>
    public class DelimitedReader : IRecordReader
    {
        private readonly char _delim;

        private readonly char? _escapeChar;

        private readonly char? _lineContinuationChar;

        private readonly char? _recordTerminator;

        private readonly CommentReader _commentReader;

        private readonly TextReader _in;

        private IList<string> _fieldList = new List<string>();

        private int _lineNumber;

        private int _recordLineNumber;

        private bool _skipLineFeed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedReader"/> class.
        /// </summary>
        /// <param name="textReader">the input stream to read from</param>
        public DelimitedReader(TextReader textReader)
            : this(textReader, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedReader"/> class.
        /// </summary>
        /// <param name="textReader">the input stream to read from</param>
        /// <param name="delimiter">the field delimiting character</param>
        public DelimitedReader(TextReader textReader, char delimiter)
            : this(textReader, new DelimitedParserConfiguration(delimiter))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedReader"/> class.
        /// </summary>
        /// <param name="textReader">the input stream to read from</param>
        /// <param name="config">the reader configuration settings or <code>null</code> to use default values</param>
        public DelimitedReader(TextReader textReader, DelimitedParserConfiguration config)
        {
            if (config == null)
                config = new DelimitedParserConfiguration();

            _in = textReader;
            _delim = config.Delimiter;

            _escapeChar = config.Escape;
            if (_escapeChar != null && _delim == _escapeChar)
                throw new BeanIOConfigurationException("The field delimiter canot match the escape character");

            _lineContinuationChar = config.LineContinuationCharacter;
            if (_lineContinuationChar != null && _lineContinuationChar == _delim)
                throw new BeanIOConfigurationException("The field delimiter cannot match the line continuation character");

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
                    throw new BeanIOConfigurationException("Record terminator must be a single character");
                }

                if (_recordTerminator == _delim)
                    throw new BeanIOConfigurationException("The record delimiter and record terminator characters cannot match");

                if (_lineContinuationChar != null && _lineContinuationChar == _recordTerminator)
                    throw new BeanIOConfigurationException("The line continuation character and record terminator cannot match");
            }

            if (config.IsCommentEnabled)
            {
                var reader = new MarkableTextReader(textReader);
                _in = reader;
                _commentReader = new CommentReader(reader, config.Comments, _recordTerminator);
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
            get { return _recordLineNumber < 0 ? -1 : (_recordTerminator == null ? _recordLineNumber : 0); }
        }

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
        public object Read()
        {
            // fieldList is set to null when the end of stream is reached
            if (_fieldList == null)
            {
                RecordText = null;
                _recordLineNumber = -1;
                return null;
            }

            ++_lineNumber;

            // skip commented lines
            if (_commentReader != null)
            {
                int lines = _commentReader.SkipComments(_skipLineFeed);
                if (lines > 0)
                {
                    if (_commentReader.IsEof)
                    {
                        _fieldList = null;
                        RecordText = null;
                        _recordLineNumber = -1;
                        return null;
                    }

                    _lineNumber += lines;
                    _skipLineFeed = _commentReader.SkipLineFeed;
                }
            }

            var lineOffset = 0;

            // clear the field list
            _fieldList.Clear();

            var continued = false; // line continuation
            var escaped = false; // last character read matched the escape char
            var eol = false; // end of record flag
            var text = new StringBuilder(); // holds the record text being read
            var field = new StringBuilder(); // holds the latest field value being read

            int n;
            while (!eol && (n = _in.Read()) != -1)
            {
                char c = (char)n;

                // skip '\n' after a '\r'
                if (_skipLineFeed)
                {
                    _skipLineFeed = false;
                    if (c == '\n')
                    {
                        continue;
                    }
                }

                // handle line continuation
                if (continued)
                {
                    continued = false;

                    text.Append(c);

                    if (IsEndOfRecord(c, true))
                    {
                        escaped = false;
                        ++_lineNumber;
                        ++lineOffset;
                        continue;
                    }

                    if (!escaped)
                    {
                        field.Append(_lineContinuationChar);
                    }
                }
                else if (!IsEndOfRecord(c, false))
                {
                    text.Append(c);
                }

                // handle escaped characters
                if (escaped)
                {
                    escaped = false;

                    // an escape character can be used to escape itself or an end quote
                    if (c == _delim)
                    {
                        field.Append(c);
                        continue;
                    }
                    
                    if (c == _escapeChar)
                    {
                        field.Append(_escapeChar);
                        continue;
                    }
                    
                    field.Append(_escapeChar);
                }

                // default handling
                if (_escapeChar != null && c == _escapeChar)
                {
                    escaped = true;
                    if (_lineContinuationChar != null && c == _lineContinuationChar)
                    {
                        continued = true;
                    }
                }
                else if (_lineContinuationChar != null && c == _lineContinuationChar)
                {
                    continued = true;
                }
                else if (c == _delim)
                {
                    _fieldList.Add(field.ToString());
                    field = new StringBuilder();
                }
                else if (IsEndOfRecord(c, true))
                {
                    _fieldList.Add(field.ToString());
                    eol = true;
                }
                else
                {
                    field.Append(c);
                }
            }

            // update the record line number
            _recordLineNumber = _lineNumber - lineOffset;
            RecordText = text.ToString();

            // if eol is true, we're done; if not, then the end of file was reached 
            // and further validation is needed
            if (eol)
            {
                RecordText = text.ToString();
                return _fieldList.ToArray();
            }

            if (continued)
            {
                _fieldList = null;
                RecordText = null;
                _recordLineNumber = -1;
                throw new RecordIOException(string.Format("Unexpected end of stream after line continuation at line {0}", _lineNumber));
            }

            // handle last escaped char
            if (escaped)
            {
                field.Append(_escapeChar);
            }

            if (text.Length > 0)
            {
                _fieldList.Add(field.ToString());

                var record = _fieldList.ToArray();
                RecordText = text.ToString();
                _fieldList = null;
                return record;
            }

            _fieldList = null;
            RecordText = null;
            _recordLineNumber = -1;
            return null;
        }

        /// <summary>
        /// Closes this input stream.
        /// </summary>
        public void Close()
        {
            _in.Dispose();
        }

        /// <summary>
        /// Returns <code>true</code> if the given character matches the record separator. This
        /// method also updates the internal <see cref="F:_skipLineFeed"/> flag.
        /// </summary>
        /// <param name="ch">the character to test</param>
        /// <param name="skipLineFeed">the value to set if the character is a carriage return</param>
        /// <returns><code>true</code> if the character signifies the end of the record</returns>
        private bool IsEndOfRecord(char ch, bool skipLineFeed)
        {
            if (_recordTerminator == null)
            {
                if (ch == '\r')
                {
                    _skipLineFeed = skipLineFeed;
                    return true;
                }
                
                if (ch == '\n')
                {
                    return true;
                }

                return false;
            }

            return ch == _recordTerminator;
        }
    }
}
