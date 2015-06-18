﻿using Xunit;

namespace BeanIO.Parser.Xml.Marshaller
{
    public sealed class XmlMarshallerTest : XmlParserTest
    {
        private readonly StreamFactory _factory;

        public XmlMarshallerTest()
        {
            _factory = NewStreamFactory("marshaller_mapping.xml");
        }

        [Fact]
        public void TestMarshaller()
        {
            string personRecord =
                "<stream>\r\n" +
                "  <person>\r\n" +
                "    <firstName>Joe</firstName>\r\n" +
                "    <lastName>Smith</lastName>\r\n" +
                "  </person>\r\n" +
                "</stream>";

            string orderRecord =
                "<stream>\r\n" +
                "  <order>\r\n" +
                "    <id>100</id>\r\n" +
                "  </order>\r\n" +
                "</stream>";

            var m = _factory.CreateMarshaller("stream");
            var u = _factory.CreateUnmarshaller("stream");

            var person = new Beans.Person
                {
                    FirstName = "Joe",
                    LastName = "Smith"
                };

            string text = m.Marshal(person).ToString();
            Assert.Equal(personRecord, text);

            person = Assert.IsType<Beans.Person>(u.Unmarshal(personRecord));
            Assert.Equal("Joe", person.FirstName);
            Assert.Equal("Smith", person.LastName);

            person = Assert.IsType<Beans.Person>(u.Unmarshal(m.Marshal(person).AsDocument()));
            Assert.Equal("Joe", person.FirstName);
            Assert.Equal("Smith", person.LastName);

            Beans.Order order = Assert.IsType<Beans.Order>(u.Unmarshal(orderRecord));
            Assert.Equal("100", order.Id);

            order.Id = "200";
            order = Assert.IsType<Beans.Order>(u.Unmarshal(m.Marshal(order).AsDocument()));
            Assert.Equal("200", order.Id);
        }
    }
}
