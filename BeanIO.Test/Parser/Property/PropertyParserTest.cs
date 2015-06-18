using System;
using System.Collections.Generic;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Property
{
    public class PropertyParserTest : ParserTest
    {
        [Fact]
        public void TestBasic()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Property.property_mapping.xml");
            var reader = factory.CreateReader("p1", LoadReader("p1.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("id"));
                Assert.Equal(1, map["id"]);
                Assert.False(map.ContainsKey("recordType"));

                var text = new StringWriter();
                factory.CreateWriter("p1", text).Write(map);
                Assert.Equal("Header,2011-07-04" + LineSeparator, text.ToString());

                var user = Assert.IsType<User>(reader.Read());
                Assert.Equal(2, user.GetType());

                text = new StringWriter();
                factory.CreateWriter("p1", text).Write(user);
                Assert.Equal("Detail,John" + LineSeparator, text.ToString());

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("id"));
                Assert.Equal(3, map["id"]);
                Assert.True(map.ContainsKey("recordCount"));
                Assert.Equal(1, map["recordCount"]);
                Assert.False(map.ContainsKey("recordType"));

                text = new StringWriter();
                factory.CreateWriter("p1", text).Write(map);
                Assert.Equal("Trailer,1" + LineSeparator, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
