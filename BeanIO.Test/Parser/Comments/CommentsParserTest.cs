using System;
using System.Collections;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Comments
{
    public class CommentsParserTest : ParserTest
    {
        [Fact]
        public void TestCsvComments()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Comments.comments_mapping.xml");
            var reader = factory.CreateReader("c1", LoadStream("c1.txt"));
            try
            {
                var map = (IDictionary)reader.Read();
                Assert.True(map.Contains("name"));
                Assert.Equal("joe", map["name"]);
                Assert.True(map.Contains("age"));
                Assert.Equal("25", map["age"]);

                map = (IDictionary)reader.Read();
                Assert.True(map.Contains("name"));
                Assert.Equal("john", map["name"]);
                Assert.True(map.Contains("age"));
                Assert.Equal("42", map["age"]);

                map = (IDictionary)reader.Read();
                Assert.True(map.Contains("name"));
                Assert.Equal("mary", map["name"]);
                Assert.True(map.Contains("age"));
                Assert.Equal("33", map["age"]);
            }
            finally
            {
                reader.Close();
            }
        }

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.Comments.{0}", fileName);
            var asm = typeof(CommentsParserTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            return new StreamReader(resStream);
        }
    }
}
