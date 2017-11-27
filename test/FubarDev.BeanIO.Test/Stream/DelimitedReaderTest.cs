// <copyright file="DelimitedReaderTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.IO;

using BeanIO.Stream.Delimited;

using Xunit;

namespace BeanIO.Stream
{
    public class DelimitedReaderTest
    {
        [Fact]
        public void TestBasic()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory();
            var expected = new[] { "1", "2", "33", "444", string.Empty };
            DelimitedReader reader = CreateReader(factory, "1\t2\t33\t444\t\n");
            Assert.Equal(expected, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestEscapeDisabled()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory { Escape = null };
            DelimitedReader reader = CreateReader(factory, "1\\\\\t2");
            Assert.Equal(new[] { "1\\\\", "2" }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestEscapeEscape()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory { Escape = '\\' };
            DelimitedReader reader = CreateReader(factory, "1\\\\\t2");
            Assert.Equal(new[] { "1\\", "2" }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestEscapeDelimiter()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory { Escape = '\\' };
            DelimitedReader reader = CreateReader(factory, "1\\\t\t2\\");
            Assert.Equal(new[] { "1\t", "2\\" }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestEscapeOther()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory { Escape = '\\' };
            DelimitedReader reader = CreateReader(factory, "1\t2\\2");
            Assert.Equal(new[] { "1", "2\\2" }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestCustomDelimiter()
        {
            DelimitedReader reader = new DelimitedReader(new StringReader("1,2,\t3"), ',');
            Assert.Equal(new[] { "1", "2", "\t3" }, reader.Read());
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestLineContinuation()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    LineContinuationCharacter = '\\'
                };
            DelimitedReader reader = CreateReader(factory, "1,2,\\\n3,4");
            Assert.Equal(new[] { "1", "2", "3", "4" }, reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestLineContinuationCRLF()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    LineContinuationCharacter = '\\'
                };
            DelimitedReader reader = CreateReader(factory, "1,2,\\\r\n3,4");
            Assert.Equal(new[] { "1", "2", "3", "4" }, reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestLineContinuationIgnored()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    LineContinuationCharacter = '\\'
                };
            DelimitedReader reader = CreateReader(factory, "1,2,\\3,4");
            Assert.Equal(new[] { "1", "2", "\\3", "4" }, reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestLineContinuationAndEscape()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    LineContinuationCharacter = '\\',
                    Escape = '\\'
                };
            DelimitedReader reader = CreateReader(factory, "1,2,\\3,4");
            Assert.Equal(new[] { "1", "2", "\\3", "4" }, reader.Read());
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestLineNumber()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    LineContinuationCharacter = '\\'
                };
            DelimitedReader reader = CreateReader(factory, "1,2,\\\n3,4\n5,6");
            Assert.Equal(new[] { "1", "2", "3", "4" }, reader.Read());
            Assert.Equal("1,2,\\\n3,4", reader.RecordText);
            Assert.Equal(1, reader.RecordLineNumber);
            Assert.Equal(new[] { "5", "6" }, reader.Read());
            Assert.Equal(3, reader.RecordLineNumber);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestLineContinuationError()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    LineContinuationCharacter = '\\'
                };
            DelimitedReader reader = CreateReader(factory, "1,2,\\");
            Assert.Throws<RecordIOException>(() => reader.Read());
        }

        [Fact]
        public void TestCustomLineContinuation()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    LineContinuationCharacter = '#'
                };
            DelimitedReader reader = CreateReader(factory, "1,2,#\n3,4");
            Assert.Equal(reader.Read(), new[] { "1", "2", "3", "4" });
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestLF()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory();
            DelimitedReader reader = CreateReader(factory, "1\t2\n3\t4");
            Assert.Equal(reader.Read(), new[] { "1", "2" });
            Assert.Equal(reader.Read(), new[] { "3", "4" });
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestCRLF()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory();
            DelimitedReader reader = CreateReader(factory, "1\t2\r\n3\t4");
            Assert.Equal(reader.Read(), new[] { "1", "2" });
            Assert.Equal(reader.Read(), new[] { "3", "4" });
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestCR()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory();
            DelimitedReader reader = CreateReader(factory, "1\t2\r3\t4");
            Assert.Equal(reader.Read(), new[] { "1", "2" });
            Assert.Equal(reader.Read(), new[] { "3", "4" });
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestRecordTerminator()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    LineContinuationCharacter = '\\',
                    RecordTerminator = "*"
                };
            DelimitedReader reader = CreateReader(factory, "1,2,*,4\n5,6,\\*7*");
            Assert.Equal(new[] { "1", "2", string.Empty, }, reader.Read());
            Assert.Equal("1,2,", reader.RecordText);
            Assert.Equal(0, reader.RecordLineNumber);
            Assert.Equal(new[] { string.Empty, "4\n5", "6", "7" }, reader.Read());
            Assert.Equal(0, reader.RecordLineNumber);
            Assert.Null(reader.Read());
            Assert.Equal(-1, reader.RecordLineNumber);
        }

        [Fact]
        public void TestClose()
        {
            DelimitedReader reader = new DelimitedReader(new StringReader(string.Empty));
            reader.Close();
        }

        [Fact]
        public void TestDelimiterCannotMatchContinuation()
        {
            DelimitedParserConfiguration config = new DelimitedParserConfiguration(',') { LineContinuationCharacter = ',' };

            Assert.Throws<BeanIOConfigurationException>(() => new DelimitedReader(new StringReader(string.Empty), config));
        }

        [Fact]
        public void TestDelimiterCannotMatchEscape()
        {
            DelimitedParserConfiguration config = new DelimitedParserConfiguration(',') { Escape = ',' };

            Assert.Throws<BeanIOConfigurationException>(() => new DelimitedReader(new StringReader(string.Empty), config));
        }

        [Fact]
        public void TestMalformedRecordAtEOF()
        {
            DelimitedParserConfiguration config = new DelimitedParserConfiguration(',')
                {
                    Delimiter = ',',
                    LineContinuationCharacter = '\\'
                };

            var input = new StrictStringReader("hi\\");

            RecordIOException error = null;

            DelimitedReader reader = new DelimitedReader(input, config);
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

        private DelimitedReader CreateReader(DelimitedRecordParserFactory factory, string input)
        {
            return (DelimitedReader)factory.CreateReader(CreateInput(input));
        }

        private TextReader CreateInput(string s)
        {
            return new StringReader(s);
        }
    }
}
