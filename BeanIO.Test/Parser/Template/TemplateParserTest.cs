using System.Collections.Generic;

using Xunit;

namespace BeanIO.Parser.Template
{
    public class TemplateParserTest : ParserTest
    {
        [Fact]
        public void TestRecordTemplate()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Template.template_mapping.xml");
            var reader = factory.CreateReader("stream1", LoadReader("t1.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(1, map["id"]);
                Assert.Equal("joe", map["name"]);
                Assert.Equal('M', map["gender"]);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestBeanTemplate()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Template.template_mapping.xml");
            var reader = factory.CreateReader("stream2", LoadReader("t1.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal('M', map["gender"]);
                Assert.NotNull(map["bean"]);
                map = Assert.IsType<Dictionary<string, object>>(map["bean"]);
                Assert.Equal(1, map["id"]);
                Assert.Equal("joe", map["name"]);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestIncludeTemplateFromRecord()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Template.template_mapping.xml");
            var reader = factory.CreateReader("stream3", LoadReader("t3.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(1, map["id"]);
                Assert.Equal("joe", map["firstName"]);
                Assert.Equal("smith", map["lastName"]);
                Assert.Equal('M', map["gender"]);
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
