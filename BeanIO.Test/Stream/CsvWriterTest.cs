using System;
using System.IO;

using BeanIO.Stream.Csv;

using Xunit;

namespace BeanIO.Stream
{
    public class CsvWriterTest
    {
        private static readonly string _lineSeparator = Environment.NewLine;

        [Fact]
        public void TestDefaultConfiguration()
        {
            CsvRecordParserFactory factory = new CsvRecordParserFactory();
            StringWriter text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write(new[] { "value1", "\"value2\"", "value,3" });
            Assert.Equal("value1,\"\"\"value2\"\"\",\"value,3\"" + _lineSeparator, text.ToString());
        }

        [Fact]
        public void TestCustomConfiguration()
        {
            CsvRecordParserFactory factory = new CsvRecordParserFactory()
                {
                    Delimiter = ':',
                    Quote = '\'',
                    Escape = '\\',
                    RecordTerminator = string.Empty,
                };
            var text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write(new[] { "value1", "'value2'", "value:3" });
            Assert.Equal("value1:'\\'value2\\'':'value:3'", text.ToString());
        }

        [Fact]
        public void TestAlwaysQuote()
        {
            CsvRecordParserFactory factory = new CsvRecordParserFactory()
                {
                    Quote = '\'',
                    Escape = '\\',
                    RecordTerminator = string.Empty,
                    AlwaysQuote = true,
                };
            var text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write(new[] { "value1", "'value2'", "value,3" });
            Assert.Equal("'value1','\\'value2\\'','value,3'", text.ToString());
        }

        [Fact]
        public void TestMultiline()
        {
            CsvRecordParserFactory factory = new CsvRecordParserFactory()
                {
                    Quote = '\'',
                    Escape = '\\',
                    RecordTerminator = string.Empty,
                };
            var text = new StringWriter();
            var writer = (CsvWriter)factory.CreateWriter(text);
            writer.Write(new[] { "value1", "value\n2", "value\r3", "value\r\n4" });
            Assert.Equal("value1,'value\n2','value\r3','value\r\n4'", text.ToString());
            writer.Write(new[] { "value1", "value2" });
            Assert.Equal(5, writer.LineNumber);
        }

        [Fact]
        public void TestFlushAndClose()
        {
            CsvRecordParserFactory factory = new CsvRecordParserFactory()
            {
                Quote = '\'',
                Escape = '\\',
                RecordTerminator = string.Empty,
            };
            var text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write(new[] { "v" });
            writer.Flush();
            Assert.Equal("v", text.ToString());
            writer.Close();
        }
    }
}
