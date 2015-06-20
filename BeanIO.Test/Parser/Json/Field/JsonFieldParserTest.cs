using System.Collections.Generic;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Json.Field
{
    public sealed class JsonFieldParserTest : ParserTest
    {
        private readonly StreamFactory _factory;

        public JsonFieldParserTest()
        {
            _factory = NewStreamFactory("jsonField_mapping.xml");
        }

        [Fact]
        public void TestFieldSimple()
        {
            var reader = _factory.CreateReader("stream", LoadReader("jf1.txt"));

            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal("Joe", map["firstName"]);
                Assert.Equal("Johnson", map["lastName"]);
                Assert.Equal("20", map["age"]);
                Assert.Equal(1, map["number"]);
                Assert.Equal(true, map["healthy"]);
                Assert.Equal(new[] { 10, 20 }, Assert.IsType<List<int>>(map["array"]));

                var text = new StringWriter();
                _factory.CreateWriter("stream", text).Write(map);
                Assert.Equal(Load("jf1.txt"), text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
