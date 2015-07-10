using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace BeanIO.Stream.Csv
{
    /// <summary>
    /// A combined <see cref="IRecordMarshaller"/> and <see cref="IRecordUnmarshaller"/> implementation for CSV formatted records.
    /// </summary>
    public class CsvRecordParser : IRecordMarshaller, IRecordUnmarshaller
    {
        private readonly char _delim;

        private readonly char _quote;

        private readonly char _endQuote;

        private readonly char? _escape;

        private readonly bool _whitespaceAllowed;

        private readonly bool _unquotedQuotesAllowed;

        private readonly bool _alwaysQuote;

        private readonly IList<string> _fieldList = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvRecordParser"/> class.
        /// </summary>
        public CsvRecordParser()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvRecordParser"/> class.
        /// </summary>
        /// <param name="config">The parser configuration</param>
        public CsvRecordParser(CsvParserConfiguration config)
        {
            if (config == null)
                config = new CsvParserConfiguration();

            _delim = config.Delimiter;
            if (_delim == ' ')
                throw new BeanIOConfigurationException(string.Format("The CSV field delimiter '{0}' is not supported", _delim));
            _quote = config.Quote;
            _endQuote = _quote;
            if (_quote == _delim)
                throw new BeanIOConfigurationException("The CSV field delimiter cannot match the character used for the quotation mark.");
            _whitespaceAllowed = config.IsWhitespaceAllowed;
            _unquotedQuotesAllowed = config.UnquotedQuotesAllowed;
            _escape = config.Escape;
            if (_escape != null && _escape == _delim)
                throw new BeanIOConfigurationException("The CSV field delimiter cannot match the escape character.");
            _alwaysQuote = config.AlwaysQuote;
        }

        /// <summary>
        /// Marshals a single record object to a <code>String</code>.
        /// </summary>
        /// <param name="record">Record the record object to marshal</param>
        /// <returns>The marshalled record text</returns>
        public string Marshal(object record)
        {
            return Marshal((string[])record);
        }

        /// <summary>
        /// Marshals a single record object to a <code>String</code>.
        /// </summary>
        /// <param name="record">Record the record object to marshal</param>
        /// <returns>The marshalled record text</returns>
        public string Marshal(string[] record)
        {
            var text = new StringBuilder();

            var pos = 0;
            foreach (var field in record)
            {
                if (pos++ > 0)
                    text.Append(_delim);

                var cs = field.ToCharArray();
                var quoted = _alwaysQuote || cs.MustQuote(_delim, _quote);
                if (quoted)
                    text.Append(_quote);

                foreach (var c in cs)
                {
                    if (c == _endQuote || (_escape != null && c == _escape))
                        text.Append(_escape);
                    text.Append(c);
                }

                if (quoted)
                    text.Append(_quote);
            }

            return text.ToString();
        }

        /// <summary>
        /// Unmarshals a single record.
        /// </summary>
        /// <param name="text">The record text to unmarshal</param>
        /// <returns>The unmarshalled record object</returns>
        [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP2101:MethodMustNotContainMoreLinesThan", Justification = "Reviewed. Suppression is OK here.")]
        public object Unmarshal(string text)
        {
            _fieldList.Clear();

            var field = new StringBuilder();
            var state = 0;          // current state
            var whitespace = 0;
            var escaped = false;    // last character read matched the escape char

            foreach (var c in text.ToCharArray())
            {
                // handle escaped characters
                if (escaped)
                {
                    escaped = false;

                    // an escape character can be used to escape itself or an end quote
                    if (c == _endQuote || (_escape != null && c == _escape))
                    {
                        field.Append(c);
                        continue;
                    }

                    // the field was ended if escape and endQuote are the same character such as "
                    if (_escape != null && _escape == _endQuote)
                    {
                        _fieldList.Add(field.ToString());
                        field = new StringBuilder();
                        state = 10;
                    }
                }

                switch (state)
                {
                    case 0:
                        // initial state (beginning of line, or next value)
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
                        else if (c == ' ')
                        {
                            if (!_whitespaceAllowed)
                            {
                                field.Append(c);
                                state = 2; // look for next delimiter
                            }
                            else
                            {
                                ++whitespace;
                            }
                        }
                        else
                        {
                            field.Append(whitespace.ToWhitespace());
                            whitespace = 0;
                            field.Append(c);
                            state = 2; // look for next delimiter
                        }

                        break;

                    case 1:
                        // quoted field, look for trailing quote at end of field
                        if (_escape != null && c == _escape)
                        {
                            escaped = true;
                        }
                        else if (c == _endQuote)
                        {
                            _fieldList.Add(field.ToString());
                            field = new StringBuilder();
                            state = 10; // look for next delimiter
                        }
                        else
                        {
                            field.Append(c);
                        }

                        break;

                    case 2:
                        // unquoted field, look for next delimiter
                        if (c == _delim)
                        {
                            _fieldList.Add(field.ToString());
                            field = new StringBuilder();
                            state = 0;
                        }
                        else if (c == _quote && !_unquotedQuotesAllowed)
                        {
                            throw new RecordIOException(string.Format("Quotation character '{0}' must be quoted", _quote));
                        }
                        else
                        {
                            field.Append(c);
                        }

                        break;

                    case 10:
                        // quoted field, after final quote read
                        if (c == ' ')
                        {
                            if (!_whitespaceAllowed)
                                throw new RecordIOException("Invalid whitespace found outside of a quoted field");
                        }
                        else if (c == _delim)
                        {
                            state = 0;
                        }
                        else
                        {
                            throw new RecordIOException("Invalid character found outside of quoted field");
                        }

                        break;
                }
            }

            // handle escaped mode
            if (escaped && _escape == _endQuote)
            {
                _fieldList.Add(field.ToString());
                state = 10;
            }

            // validate current state...
            switch (state)
            {
                case 0:
                    _fieldList.Add(whitespace.ToWhitespace());
                    break;
                case 1:
                    throw new RecordIOException("Expected end quote before end of record");
                case 2:
                    _fieldList.Add(field.ToString());
                    break;
                case 10:
                    break;
            }

            var record = _fieldList.ToArray();
            return record;
        }
    }
}
