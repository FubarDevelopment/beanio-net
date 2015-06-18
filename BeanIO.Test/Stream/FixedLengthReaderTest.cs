using System.IO;

using BeanIO.Stream.FixedLength;

using Xunit;

namespace BeanIO.Stream
{
    public class FixedLengthReaderTest
    {
        [Fact]
        public void TestBasic()
        {
            var factory = new FixedLengthRecordParserFactory();
            var reader = CreateReader(factory, "1111\n2222");
            Assert.Equal("1111", reader.Read());
            Assert.Equal("2222", reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestLineContinuation()
        {
            var factory = new FixedLengthRecordParserFactory { LineContinuationCharacter = '\\' };
            var reader = CreateReader(factory, "11\\\n22\n33\\\r\n\\44");
            Assert.Equal("1122", reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Equal("33\\44", reader.Read());
            Assert.Equal(3, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestCustomLineContinuationChar()
        {
            var factory = new FixedLengthRecordParserFactory { LineContinuationCharacter = '#' };
            var reader = CreateReader(factory, "11#\n22\n33");
            Assert.Equal("1122", reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Equal("33", reader.Read());
            Assert.Equal(3, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact] // (expected = RecordIOException.class)
        public void TestLineContinuationError()
        {
            var factory = new FixedLengthRecordParserFactory { LineContinuationCharacter = '\\' };
            var reader = CreateReader(factory, "11\\");
            Assert.Throws<RecordIOException>(() => reader.Read());
        }

        [Fact]
        public void TestCR()
        {
            var factory = new FixedLengthRecordParserFactory();
            var reader = CreateReader(factory, "1111\r2222");
            Assert.Equal("1111", reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Equal("2222", reader.Read());
            Assert.Equal(2, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestLF()
        {
            var factory = new FixedLengthRecordParserFactory();
            var reader = CreateReader(factory, "1111\n2222");
            Assert.Equal("1111", reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Equal("2222", reader.Read());
            Assert.Equal(2, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestCRLF()
        {
            var factory = new FixedLengthRecordParserFactory();
            var reader = CreateReader(factory, "1111\r\n2222");
            Assert.Equal("1111", reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Equal("2222", reader.Read());
            Assert.Equal(2, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestRecordTerminator()
        {
            var factory = new FixedLengthRecordParserFactory
                {
                    LineContinuationCharacter = '\\',
                    RecordTerminator = "*"
                };
            var reader = CreateReader(factory, "11\\*22*33\\44*");
            Assert.Equal("1122", reader.Read());
            Assert.Equal(0, reader.RecordLineNumber);
            Assert.Equal("33\\44", reader.Read());
            Assert.Equal(0, reader.RecordLineNumber);
            Assert.Null(reader.Read());
            Assert.Equal(-1, reader.RecordLineNumber);
        }

        [Fact]
        public void TestEmpty()
        {
            Assert.Null(new FixedLengthReader(new StringReader(string.Empty)).Read());
        }

        [Fact]
        public void TestComments()
        {
            FixedLengthParserConfiguration config = new FixedLengthParserConfiguration
                {
                    Comments = new[] { "#", "//" },
                    RecordTerminator = "+"
                };

            StringReader input = new StringReader(
                "# comment+" +
                "one+" +
                "/+" +
                "+" +
                "// ignored+" +
                "//++");

            var reader = new FixedLengthReader(input, config);
            Assert.Equal("one", reader.Read());
            Assert.Equal("/", reader.Read());
            Assert.Equal(string.Empty, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestMalformedRecordAtEOF()
        {
            FixedLengthParserConfiguration config = new FixedLengthParserConfiguration { LineContinuationCharacter = '\\' };

            StrictStringReader input = new StrictStringReader("hi\\");

            RecordIOException error = null;

            var reader = new FixedLengthReader(input, config);
            try
            {
                reader.Read();
            }
            catch (RecordIOException ex)
            {
                error = ex;
            }

            Assert.NotNull(error);
            Assert.Null(reader.Read());
        }

        private FixedLengthReader CreateReader(FixedLengthRecordParserFactory factory, string input)
        {
            return (FixedLengthReader)factory.CreateReader(CreateInput(input));
        }

        private TextReader CreateInput(string s)
        {
            return new StringReader(s);
        }
    }
}
