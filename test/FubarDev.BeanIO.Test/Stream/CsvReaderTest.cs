// <copyright file="CsvReaderTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

using BeanIO.Stream.Csv;

using Xunit;

namespace BeanIO.Stream
{
    public class CsvReaderTest
    {
        private readonly CsvRecordParserFactory _factory = new CsvRecordParserFactory();

        [Fact]
        public void TestEmptyFile()
        {
            var reader = CreateReader(string.Empty);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestNewLine()
        {
            var reader = CreateReader("\n");
            Assert.Equal(new[] { string.Empty }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestDelimiter()
        {
            var reader = CreateReader("   1,2,3");
            Assert.Equal(new[] { "   1", "2", "3" }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestDelimiterOnly()
        {
            var reader = CreateReader(",,\n");
            Assert.Equal(new[] { string.Empty, string.Empty, string.Empty }, reader.Read());
            Assert.Null(reader.Read());

            reader = CreateReader(",");
            Assert.Equal(new[] { string.Empty, string.Empty }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestEscapedDelimiter()
        {
            var reader = CreateReader("\"1,\",2");
            Assert.Equal(new[] { "1,", "2" }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestEscapedQuote()
        {
            var reader = CreateReader("\"1,\"\"\",2");
            Assert.Equal(new[] { "1,\"", "2" }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestQuotedFields()
        {
            var reader = CreateReader("\"1\",\"\",\"3\"");
            Assert.Equal(new[] { "1", string.Empty, "3" }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestCharacterOutOfQuotedField()
        {
            Assert.Throws<RecordIOException>(() => CreateReader("\"1\",\"\",\"3\"2\n\r1,2").Read());
        }

        [Fact]
        public void TestSpaceOutofQuotedField()
        {
            Assert.Throws<RecordIOException>(() => CreateReader("\"1\",\"\",\"3\" \n\r1,2").Read());
        }

        [Fact]
        public void TestRecover()
        {
            var expected = new[] { string.Empty, "2" };
            CsvReader reader = CreateReader("\"1\",\"\",\"3\" 2\n,2");
            Assert.Throws<RecordIOException>(() => reader.Read());
            Assert.Equal(expected, reader.Read());
        }

        [Fact]
        public void TestCRLF()
        {
            var expected = new[] { "4", "5", "6" };
            CsvReader reader = CreateReader("1,2,3\r\n4,5,6\r\n");
            reader.Read();
            Assert.Equal(expected, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestCR()
        {
            var expected = new[] { "4", "5", "6" };
            CsvReader reader = CreateReader("1,2,3\r4,5,6\r");
            reader.Read();
            Assert.Equal(expected, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestLF()
        {
            var expected = new[] { "4", "5", "6" };
            CsvReader reader = CreateReader("1,2,3\n4,5,6\n");
            reader.Read();
            Assert.Equal(expected, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestCustomDelimiter()
        {
            var factory = new CsvRecordParserFactory
                {
                    Delimiter = '|'
                };
            string[] expected = { "1", "2", "3" };
            CsvReader reader = CreateReader(factory, "\"1\"|2|3");
            Assert.Equal(expected, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestCustomQuote()
        {
            var factory = new CsvRecordParserFactory
                {
                    Quote = '\'',
                };
            string[] expected = { "1", " 234 ", "5" };
            CsvReader reader = CreateReader(factory, "'1',' 234 ',5\n");
            Assert.Equal(expected, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestCustomEscape()
        {
            var factory = new CsvRecordParserFactory
                {
                    Quote = '\'',
                    Escape = '\\',
                };
            var expected = new[] { "1", " '23\\4' ", "5\\\\" };
            CsvReader reader = CreateReader(factory, "'1',' \\'23\\\\4\\' ',5\\\\\n");
            Assert.Equal(expected, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestMultiline()
        {
            var factory = new CsvRecordParserFactory
                {
                    Quote = '\'',
                    IsMultilineEnabled = true,
                };
            var expected = new[] { "12\n3", "4\r\n5" };
            CsvReader reader = CreateReader(factory, "'12\n3','4\r\n5'\n'6',7");
            Assert.Equal(expected, reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Equal(reader.Read(), new[] { "6", "7" });
            Assert.Equal(4, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestWhitespaceeAllowed()
        {
            var factory = new CsvRecordParserFactory
                {
                    Quote = '\'',
                    IsWhitespaceAllowed = true,
                };
            var expected = new[] { "1", "2" };
            CsvReader reader = CreateReader(factory, " '1' , '2'  \n");
            Assert.Equal(expected, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestEscapeDisabled()
        {
            var factory = new CsvRecordParserFactory
                {
                    Quote = '\'',
                    Escape = null,
                };
            var expected = new[] { "1\"", "2" };
            CsvReader reader = CreateReader(factory, "'1\"','2'\n");
            Assert.Equal(expected, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestUnquotedQuoteAllowed()
        {
            var factory = new CsvRecordParserFactory
                {
                    Quote = '\'',
                    UnquotedQuotesAllowed = true,
                };
            var expected = new[] { "1\"1", "2" };
            CsvReader reader = CreateReader(factory, "1\"1,2");
            Assert.Equal(expected, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestMissingQuoteEOF()
        {
            StringReader text = new StringReader("field1,\"field2");

            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Delimiter = ',',
                    Escape = null,
                    IsMultilineEnabled = false,
                };

            Assert.Throws<RecordIOException>(() => new CsvReader(text, config).Read());
        }

        [Fact]
        public void TestMissingQuoteEOL()
        {
            StringReader text = new StringReader("field1,\"field2\nfield1");

            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Delimiter = ',',
                    Escape = null,
                    IsMultilineEnabled = false,
                };

            CsvReader reader = new CsvReader(text, config);
            Assert.Throws<RecordIOException>(() => reader.Read());
            Assert.Equal(new[] { "field1" }, reader.Read());
            Assert.Equal(2, reader.RecordLineNumber);
        }

        [Fact]
        public void TestRecoverSkipLF()
        {
            StringReader text = new StringReader(
                "field1,\"field2\" ,field3\r\nfield1");

            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Delimiter = ',',
                    Escape = null,
                    IsMultilineEnabled = false,
                };
            CsvReader reader = new CsvReader(text, config);
            Assert.Throws<RecordIOException>(() => reader.Read());
            Assert.Equal(new[] { "field1" }, reader.Read());
        }

        [Fact]
        public void TestQuoteIsDelimiter()
        {
            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Delimiter = ',',
                    Quote = ',',
                };
            Assert.Throws<BeanIOConfigurationException>(() => new CsvReader(new StringReader(string.Empty), config));
        }

        [Fact]
        public void TestQuoteIsEscape()
        {
            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Delimiter = ',',
                    Escape = ',',
                };
            Assert.Throws<BeanIOConfigurationException>(() => new CsvReader(new StringReader(string.Empty), config));
        }

        [Fact]
        public void TestCreateWhitespace()
        {
            var factory = new CsvRecordParserFactory()
                {
                    IsWhitespaceAllowed = true,
                    Quote = '\'',
                };
            var expected = new[] { "   1", "2", "  3  " };
            CsvReader reader = CreateReader(factory, "   1,2,  3  ");
            Assert.Equal(expected, reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestComments()
        {
            var comments = new[] { "#", "$$", "--" };

            CsvParserConfiguration config = new CsvParserConfiguration()
                {
                    Comments = comments,
                };

            StringReader input = new StringReader(
                "# Comment\n" +
                "1\r\n" +
                "-1\r\n" +
                "--Comment\r\n" +
                "2\n" +
                "#");

            CsvReader reader = new CsvReader(input, config);
            Assert.Equal(new[] { "1" }, reader.Read());
            Assert.Equal(2, reader.RecordLineNumber);
            Assert.Equal(new[] { "-1" }, reader.Read());
            Assert.Equal(3, reader.RecordLineNumber);
            Assert.Equal(new[] { "2" }, reader.Read());
            Assert.Equal(5, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestMalformedRecordAtEOF()
        {
            var input = new StrictStringReader("\"hello,ma");

            CsvReader reader = new CsvReader(input);
            var error = Assert.Throws<RecordIOException>(() => reader.Read());

            Assert.NotNull(error);
            Assert.Null(reader.Read());
        }

        private CsvReader CreateReader(CsvRecordParserFactory factory, string input)
        {
            return (CsvReader)factory.CreateReader(CreateInput(input));
        }

        private CsvReader CreateReader(string input)
        {
            return (CsvReader)_factory.CreateReader(CreateInput(input));
        }

        private TextReader CreateInput(string s)
        {
            return new StringReader(s);
        }
    }
}
