using System;

using Xunit;

namespace BeanIO.Parser.Skip
{
    public class SkippingParserTest : ParserTest
    {
        [Fact]
        public void TestSkip()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Skip.skip_mapping.xml");
            var reader = factory.CreateReader("s1", LoadReader("s1.txt"));
            try
            {
                Assert.Equal(0, reader.Skip(0));
                Assert.Equal(4, reader.Skip(4));

                reader.Read();
                Assert.Equal("Detail", reader.RecordName);
                reader.Read();
                Assert.Equal("Trailer", reader.RecordName);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestSkipPastEOF()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Skip.skip_mapping.xml");
            var reader = factory.CreateReader("s1", LoadReader("s1.txt"));
            try
            {
                Assert.Equal(6, reader.Skip(10));
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
