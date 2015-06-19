using System;
using System.Collections.Generic;
using System.IO;

using NodaTime;

using Xunit;
namespace BeanIO.Parser.Xml.TypeHandler
{
    public sealed class XmlTypeHandlerTest : XmlParserTest
    {
        private readonly StreamFactory _factory;

        public XmlTypeHandlerTest()
        {
            _factory = NewStreamFactory("typehandler_mapping.xml");
        }

        [Fact]
        public void TestFieldTypesAndNillable()
        {
            var reader = _factory.CreateReader("stream", LoadReader("th1_in.xml"));
            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream", s);
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(new LocalDate(2011, 1, 1), map["date"]);
                Assert.Equal(new DateTime(2011, 1, 1, 13, 45, 0), map["datetime"]);
                Assert.Equal(new LocalTime(11, 12, 13), map["time"]);
                Assert.Equal(new LocalDate(2011, 2, 1), map["customdate"]);
                Assert.Equal(true, map["boolean"]);

                writer.Write(map);
                writer.Close();

                Assert.Equal(Load("th1_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
