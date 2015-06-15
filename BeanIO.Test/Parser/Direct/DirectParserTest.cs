using Xunit;

namespace BeanIO.Parser.Direct
{
    public class DirectParserTest : ParserTest
    {
        [Fact]
        public void TestPadding()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Direct.direct_mapping.xml");
            var u = factory.CreateUnmarshaller("d1");
            var p = (DirectUser)u.Unmarshal("george,true");
            Assert.Equal("george", p.FirstName);
            Assert.True(p.Enabled);
        }
    }
}
