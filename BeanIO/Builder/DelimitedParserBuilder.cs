using BeanIO.Internal.Config;
using BeanIO.Stream;
using BeanIO.Stream.Delimited;

namespace BeanIO.Builder
{
    /// <summary>
    /// Builder for delimited parsers.
    /// </summary>
    public class DelimitedParserBuilder : IParserBuilder
    {
        private readonly DelimitedRecordParserFactory _parser = new DelimitedRecordParserFactory();

        public DelimitedParserBuilder()
        {
        }

        public DelimitedParserBuilder(char delimiter)
        {
            _parser.Delimiter = delimiter;
        }

        public DelimitedParserBuilder Delimiter(char delimiter)
        {
            _parser.Delimiter = delimiter;
            return this;
        }

        public DelimitedParserBuilder RecordTerminator(string terminator)
        {
            _parser.RecordTerminator = terminator;
            return this;
        }

        public DelimitedParserBuilder EnableEscape(char escape)
        {
            _parser.Escape = escape;
            return this;
        }

        public DelimitedParserBuilder EnableLineContinuation(char c)
        {
            _parser.LineContinuationCharacter = c;
            return this;
        }

        public DelimitedParserBuilder EnableComments(params string[] comments)
        {
            _parser.Comments = comments;
            return this;
        }

        public BeanConfig<IRecordParserFactory> Build()
        {
            var config = new BeanConfig<IRecordParserFactory>(() => _parser);
            return config;
        }
    }
}
