using System;
using System.IO;

using BeanIO.Stream.FixedLength;

using Xunit;

namespace BeanIO.Stream
{
    public class FixedLengthWriterTest
    {
        private static readonly string _lineSep = Environment.NewLine;

        [Fact]
        public void TestDefaultConfiguration()
        {
            var text = new StringWriter();
            FixedLengthWriter writer = new FixedLengthWriter(text);
            writer.Write("value1  value2");
            Assert.Equal("value1  value2" + _lineSep, text.ToString());
        }

        [Fact]
        public void TestDefaultFactoryConfiguration()
        {
            var factory = new FixedLengthRecordParserFactory();
            var text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write("value1  value2");
            Assert.Equal("value1  value2" + _lineSep, text.ToString());
        }

        [Fact]
        public void TestCustomFactoryConfiguration()
        {
            var factory = new FixedLengthRecordParserFactory { RecordTerminator = string.Empty };
            var text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write("value1  value2");
            Assert.Equal("value1  value2", text.ToString());
        }

        [Fact]
        public void TestFlushAndClose()
        {
            var factory = new FixedLengthRecordParserFactory { RecordTerminator = string.Empty };
            var text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write("v");
            writer.Flush();
            Assert.Equal("v", text.ToString());
            writer.Close();
        }
    }
}
