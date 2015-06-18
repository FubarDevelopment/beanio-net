using System.Collections.Generic;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Xml.Record
{
    public sealed class XmlRecordTest : XmlParserTest
    {
        private readonly StreamFactory _factory;

        public XmlRecordTest()
        {
            _factory = NewStreamFactory("record_mapping.xml");
        }

        /// <summary>
        /// Test a nillable child bean.
        /// </summary>
        [Fact]
        public void TestRecordClassIsCollection()
        {
            var reader = _factory.CreateReader("stream", LoadReader("r1_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream", s);

            try
            {
                var list = Assert.IsType<List<object>>(reader.Read());
                Assert.Collection(
                    list,
                    item => Assert.Equal("John", item),
                    Assert.Null,
                    item => Assert.Equal(22, item));

                writer.Write(list);
                writer.Close();
                Assert.Equal(Load("r1_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
