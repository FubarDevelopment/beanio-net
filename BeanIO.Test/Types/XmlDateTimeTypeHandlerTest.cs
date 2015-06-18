using System;
using System.Xml;

using BeanIO.Types.Xml;

using Xunit;

namespace BeanIO.Types
{
    public class XmlDateTimeTypeHandlerTest
    {
        [Fact]
        public void TestTime()
        {
            var handler = new XmlDateTimeTypeHandler();
            var date = Assert.IsType<DateTime>(handler.Parse("2000-12-02T15:14:13"));
            Assert.Equal(new DateTime(2000, 12, 2, 15, 14, 13), date);
            Assert.Equal("2000-12-02T15:14:13", handler.Format(date));
        }

        [Fact]
        public void TestTimeWithMillisecondsAndTimezone()
        {
            var handler = new XmlDateTimeOffsetTypeHandler()
                {
                    OutputMilliseconds = true,
                    TimeZoneId = "GMT+1:00",
                };
            var date = Assert.IsType<DateTimeOffset>(handler.Parse("2000-01-31T08:04:03.1234+01:00"));
            Assert.Equal(XmlConvert.ToDateTimeOffset("2000-01-31T08:04:03.1234+01:00"), date);
            Assert.Equal("2000-01-31T08:04:03.123+01:00", handler.Format(date));
        }

        [Fact]
        public void TestInvalidTime()
        {
            var handler = new XmlDateTimeTypeHandler();
            Assert.Throws<TypeConversionException>(() => handler.Parse("01:02:03"));
        }

        [Fact]
        public void TestInvalidTimeWithTimezone()
        {
            var handler = new XmlDateTimeOffsetTypeHandler()
            {
                IsTimeZoneAllowed = false,
            };
            Assert.Throws<TypeConversionException>(() => handler.Parse("2000-01-31T08:04:03.1234+01:00"));
        }

        [Fact]
        public void TestDatatypeLenient()
        {
            var handler = new XmlDateTimeTypeHandler()
                {
                    IsLenient = true,
                };
            var date = Assert.IsType<DateTime>(handler.Parse("15:14:13"));
            Assert.Equal(new DateTime(1970, 1, 1, 15, 14, 13, 0), date);
            Assert.Equal("1970-01-01T15:14:13", handler.Format(date));
        }
    }
}
