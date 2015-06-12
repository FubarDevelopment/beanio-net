using System;

using Xunit;

namespace BeanIO.Types
{
    public class CharacterTypeHandlerTest
    {
        [Fact]
        public void TestPars()
        {
            var handler = new CharacterTypeHandler();
            Assert.Equal('V', handler.Parse("V"));
            Assert.Null(handler.Parse(null));
            Assert.Null(handler.Parse(string.Empty));
        }

        [Fact]
        public void TestParseInvalid()
        {
            var handler = new CharacterTypeHandler();
            Assert.Throws<FormatException>(() => handler.Parse("value"));
        }

        [Fact]
        public void TestFormat()
        {
            var handler = new CharacterTypeHandler();
            Assert.Equal("V", handler.Format('V'));
            Assert.Equal(string.Empty, handler.Format(string.Empty));
            Assert.Null(handler.Format(null));
        }
    }
}
