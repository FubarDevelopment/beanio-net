using BeanIO.Internal.Config;
using BeanIO.Stream;
using BeanIO.Stream.Csv;

namespace BeanIO.Builder
{
    /// <summary>
    /// Builder for CSV parsers
    /// </summary>
    public class CsvParserBuilder : IParserBuilder
    {
        private readonly CsvRecordParserFactory _parser = new CsvRecordParserFactory();

        public CsvParserBuilder Delimiter(char delimiter)
        {
            _parser.Delimiter = delimiter;
            return this;
        }

        public CsvParserBuilder Quote(char quote)
        {
            _parser.Quote = quote;
            return this;
        }

        public CsvParserBuilder Escape(char escape)
        {
            _parser.Escape = escape;
            return this;
        }

        public CsvParserBuilder RecordTerminator(string terminator)
        {
            _parser.RecordTerminator = terminator;
            return this;
        }

        public CsvParserBuilder EnableComments(params string[] comments)
        {
            _parser.Comments = comments;
            return this;
        }

        public CsvParserBuilder EnableMultiline()
        {
            _parser.IsMultilineEnabled = true;
            return this;
        }

        public CsvParserBuilder AllowUnquotedWhitespace()
        {
            _parser.IsWhitespaceAllowed = true;
            return this;
        }

        public CsvParserBuilder AllowUnquotedQuotes()
        {
            _parser.UnquotedQuotesAllowed = true;
            return this;
        }

        public CsvParserBuilder AlwaysQuote()
        {
            _parser.AlwaysQuote = true;
            return this;
        }

        public BeanConfig<IRecordParserFactory> Build()
        {
            var config = new BeanConfig<IRecordParserFactory>(() => _parser);
            return config;
        }
    }
}
