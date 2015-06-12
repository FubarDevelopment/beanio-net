using System;
using System.Globalization;

using Xunit;

namespace BeanIO.Types
{
    public class NumberTypeHandlerTest
    {
        [Fact]
        public void TestParseInvalid()
        {
            var handler = new IntegerTypeHandler();
            Assert.Throws<FormatException>(() => handler.Parse("abc"));
        }

        [Fact]
        public void TestParseHexPattern()
        {
            var handler = new IntegerTypeHandler
                {
                    Pattern = Tuple.Create(NumberStyles.HexNumber, "X")
                };
            Assert.Equal(16, handler.Parse("10"));
        }

        [Fact]
        public void TestParseInvalidIncomplete()
        {
            var handler = new IntegerTypeHandler
            {
                Pattern = Tuple.Create(NumberStyles.Any, "0")
            };
            Assert.Throws<FormatException>(() => handler.Parse("10a"));
        }
    }
}
