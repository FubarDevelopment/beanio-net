using System;
using System.Diagnostics;
using System.IO;

using Xunit;

namespace BeanIO.Parser.IgnoreUnidentifiedRecords
{
    public class IgnoreUnidentifiedRecordsParserTest : ParserTest
    {
        [Fact]
        public void TestIgnoreUnidentifiedRecords()
        {
            var factory = NewStreamFactory("BeanIO.Parser.IgnoreUnidentifiedRecords.ignore_mapping.xml");
            var reader = factory.CreateReader("stream1", LoadStream("ignoreUnidentifiedRecords1.txt"));
            try
            {
                reader.Read();
                Assert.Equal("header", reader.RecordName);
                reader.Read();
                Assert.Equal("group_header", reader.RecordName);
                reader.Read();
                Assert.Equal("group_trailer", reader.RecordName);

                reader.Read();
                Assert.Equal("header", reader.RecordName);
                reader.Read();
                Assert.Equal("group_header", reader.RecordName);
                reader.Read();
                Assert.Equal("group_trailer", reader.RecordName);

                Assert.Null(reader.Read());
            }
            finally
            {
                reader.Close();
            }
        }

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.IgnoreUnidentifiedRecords.{0}", fileName);
            var asm = typeof(IgnoreUnidentifiedRecordsParserTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            Debug.Assert(resStream != null, "resStream != null");
            return new StreamReader(resStream);
        }
    }
}
