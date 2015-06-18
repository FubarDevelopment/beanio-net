using System;

using Xunit;

namespace BeanIO.Parser.IgnoreUnidentifiedRecords
{
    public class IgnoreUnidentifiedRecordsParserTest : ParserTest
    {
        [Fact]
        public void TestIgnoreUnidentifiedRecords()
        {
            var factory = NewStreamFactory("BeanIO.Parser.IgnoreUnidentifiedRecords.ignore_mapping.xml");
            var reader = factory.CreateReader("stream1", LoadReader("ignoreUnidentifiedRecords1.txt"));
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
    }
}
