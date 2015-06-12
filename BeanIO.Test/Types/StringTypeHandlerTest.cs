using Xunit;

namespace BeanIO.Types
{
    public class StringTypeHandlerTest
    {
        [Fact]
        public void TestTrim()
        {
            var handler = new StringTypeHandler
                {
                    Trim = true,
                };
            Assert.True(handler.Trim);
            Assert.Equal("value", handler.Parse("  value  "));
        }

        [Fact]
        public void TestNullIfEmpty()
        {
            var handler = new StringTypeHandler
            {
                Trim = true,
                NullIfEmpty = true
            };
            Assert.True(handler.NullIfEmpty);
            Assert.Null(handler.Parse("  "));
        }

        [Fact]
        public void TestFormatNull()
        {
            var handler = new StringTypeHandler();
            Assert.Null(handler.Format(null));
        }
    }
}
