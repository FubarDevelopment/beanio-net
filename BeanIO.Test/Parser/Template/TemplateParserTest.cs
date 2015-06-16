using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Template
{
    public class TemplateParserTest : ParserTest
    {
        [Fact]
        public void TestRecordTemplate()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Template.template_mapping.xml");
            var reader = factory.CreateReader("stream1", LoadStream("t1.txt"));
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
            var reader = factory.CreateReader("stream2", LoadStream("t1.txt"));
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
            var reader = factory.CreateReader("stream3", LoadStream("t3.txt"));
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

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.Template.{0}", fileName);
            var asm = typeof(TemplateParserTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            Debug.Assert(resStream != null, "resStream != null");
            return new StreamReader(resStream);
        }
    }
}
