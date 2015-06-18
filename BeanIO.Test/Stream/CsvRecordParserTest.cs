using BeanIO.Stream.Csv;

using Xunit;

namespace BeanIO.Stream
{
    public class CsvRecordParserTest
    {
        private readonly CsvRecordParser _parser = new CsvRecordParser();

        [Fact]
        public void TestEmptyString()
        {
            var expected = new[] { string.Empty };
            Assert.Equal(expected, Assert.IsType<string[]>(_parser.Unmarshal(string.Empty)));
        }

        [Fact]
        public void TestNewLine()
        {
            var record = "\n";
            var expected = new[] { "\n" };
            Assert.Equal(expected, Assert.IsType<string[]>(_parser.Unmarshal(record)));
        }

        [Fact]
        public void TestDelimiter()
        {
            var record = "   1,2,3";
            var expected = new[] { "   1", "2", "3" };
            Assert.Equal(expected, Assert.IsType<string[]>(_parser.Unmarshal(record)));
        }

        [Fact]
        public void TestDelimiterOnly()
        {
            var record = ",";
            var expected = new[] { string.Empty, string.Empty };
            Assert.Equal(expected, Assert.IsType<string[]>(_parser.Unmarshal(record)));
        }

        [Fact]
        public void TestEscapedDelimiter()
        {
            var record = "\"1,\",2";
            var expected = new[] { "1,", "2" };
            Assert.Equal(expected, Assert.IsType<string[]>(_parser.Unmarshal(record)));
        }

        [Fact]
        public void TestEscapedQuote()
        {
            var record = "\"1,\"\"\",2";
            var expected = new[] { "1,\"", "2" };
            Assert.Equal(expected, Assert.IsType<string[]>(_parser.Unmarshal(record)));
        }

        [Fact]
        public void TestQuotedFields()
        {
            var record = "\"1\",\"\",\"3\"";
            var expected = new[] { "1", string.Empty, "3" };
            Assert.Equal(expected, Assert.IsType<string[]>(_parser.Unmarshal(record)));
        }

        [Fact]
        public void TestCharacaterOutofQuotedField()
        {
            Assert.Throws<RecordIOException>(() => _parser.Unmarshal("\"1\",\"\",\"3\"2\n\r1,2"));
        }

        [Fact]
        public void TestSpaceOutofQuotedField()
        {
            Assert.Throws<RecordIOException>(() => _parser.Unmarshal("\"1\",\"\",\"3\" \n\r1,2"));
        }

        [Fact]
        public void TestUnquotedQuote()
        {
            Assert.Throws<RecordIOException>(() => _parser.Unmarshal("1\"1,2,3"));
        }

        [Fact]
        public void TestCustomDelimiter()
        {
            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Delimiter = '|',
                };

            var parser = new CsvRecordParser(config);
            var record = "\"1\"|2|3";
            var expected = new[] { "1", "2", "3" };
            Assert.Equal(expected, Assert.IsType<string[]>(parser.Unmarshal(record)));
        }

        [Fact]
        public void TestCustomQuote()
        {
            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Quote = '\'',
                };

            var parser = new CsvRecordParser(config);

            var record = "'1',' 234 ',5\n";
            var expected = new[] { "1", " 234 ", "5\n" };
            Assert.Equal(expected, Assert.IsType<string[]>(parser.Unmarshal(record)));
        }

        [Fact]
        public void TestCustomEscape()
        {
            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Quote = '\'',
                    Escape = '\\',
                };

            var parser = new CsvRecordParser(config);

            var record = "'1',' \\'23\\\\4\\' ',5\\\\";
            var expected = new[] { "1", " '23\\4' ", "5\\\\" };
            Assert.Equal(expected, Assert.IsType<string[]>(parser.Unmarshal(record)));
        }

        [Fact]
        public void TestWhitespaceeAllowed()
        {
            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Quote = '\'',
                    IsWhitespaceAllowed = true,
                };

            var parser = new CsvRecordParser(config);

            var record = " '1' , '2'  ";
            var expected = new[] { "1", "2" };
            Assert.Equal(expected, Assert.IsType<string[]>(parser.Unmarshal(record)));
        }

        [Fact]
        public void TestEscapeDisabled()
        {
            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Quote = '\'',
                    Escape = null,
                };

            var parser = new CsvRecordParser(config);

            var record = "'1\"','2'";
            var expected = new[] { "1\"", "2" };
            Assert.Equal(expected, Assert.IsType<string[]>(parser.Unmarshal(record)));
        }

        [Fact]
        public void TestUnquotedQuoteAllowed()
        {
            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Quote = '\'',
                    UnquotedQuotesAllowed = true,
                };

            var parser = new CsvRecordParser(config);

            var record = "1\"1,2";
            var expected = new[] { "1\"1", "2" };
            Assert.Equal(expected, Assert.IsType<string[]>(parser.Unmarshal(record)));
        }

        [Fact]
        public void TestMissingQuoteEOF()
        {
            Assert.Throws<RecordIOException>(() => _parser.Unmarshal("field1,\"field2"));
        }

        [Fact]
        public void TestQuoteIsDelimiter()
        {
            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Delimiter = ',',
                    Quote = ',',
                };

            Assert.Throws<BeanIOConfigurationException>(() => new CsvRecordParser(config));
        }

        [Fact]
        public void TestQuoteIsEscape()
        {
            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Delimiter = ',',
                    Escape = ',',
                };

            Assert.Throws<BeanIOConfigurationException>(() => new CsvRecordParser(config));
        }

        [Fact]
        public void TestCreateWhitespace()
        {
            var record = "   1,2,  3  ";
            var expected = new[] { "   1", "2", "  3  " };
            Assert.Equal(expected, Assert.IsType<string[]>(_parser.Unmarshal(record)));
        }

        [Fact]
        public void TestMarshalDefaultConfiguration()
        {
            CsvRecordParserFactory factory = new CsvRecordParserFactory();
            CsvRecordParser parser = (CsvRecordParser)factory.CreateMarshaller();
            Assert.Equal(
                "value1,\"\"\"value2\"\"\",\"value,3\"",
                parser.Marshal(new[] { "value1", "\"value2\"", "value,3" }));
        }

        [Fact]
        public void TestMarshalCustomConfiguration()
        {
            CsvRecordParserFactory factory = new CsvRecordParserFactory()
                {
                    Delimiter = ':',
                    Quote = '\'',
                    Escape = '\\',
                    RecordTerminator = string.Empty,
                };

            CsvRecordParser parser = (CsvRecordParser)factory.CreateMarshaller();
            Assert.Equal(
                "value1:'\\'value2\\'':'value:3'",
                parser.Marshal(new[] { "value1", "'value2'", "value:3" }));
        }

        [Fact]
        public void TestMarshalAlwaysQuote()
        {
            CsvRecordParserFactory factory = new CsvRecordParserFactory()
                {
                    Quote = '\'',
                    Escape = '\\',
                    RecordTerminator = string.Empty,
                    AlwaysQuote = true,
                };

            CsvRecordParser parser = (CsvRecordParser)factory.CreateMarshaller();
            Assert.Equal(
                "'value1','\\'value2\\'','value,3'",
                parser.Marshal(new[] { "value1", "'value2'", "value,3" }));
        }
    }
}
