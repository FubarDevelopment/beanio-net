using System.Collections.Generic;

using Xunit;

namespace BeanIO.Parser.Trim
{
    public class TrimTest : ParserTest
    {
        [Fact]
        public void TestLazySegment()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Trim.trim_mapping.xml");
            var u = factory.CreateUnmarshaller("s1");
            var map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal("\"jen  \",jen  ,1    "));
            Assert.Equal("jen  ", map["text"]);
            Assert.Equal("jen", map["text_trim"]);
            Assert.Equal(1, map["number"]);
        }
    }
}
