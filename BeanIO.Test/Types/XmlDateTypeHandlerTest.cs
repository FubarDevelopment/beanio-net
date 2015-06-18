using System;

using BeanIO.Types.Xml;

using NodaTime;

using Xunit;

namespace BeanIO.Types
{
    public class XmlDateTypeHandlerTest
    {
        [Fact]
        public void TestDate()
        {
            var handler = new XmlDateTypeHandler();
            var date = Assert.IsType<LocalDate>(handler.Parse("2000-01-01"));
            Assert.Equal(new LocalDate(2000, 1, 1), date);
            Assert.Equal("2000-01-01", handler.Format(date));
        }

        [Fact]
        public void TestDateWithTimezone()
        {
            var handler = new XmlDateTimeOffsetTypeHandler()
                {
                    TimeZoneId = "GMT-1:00",
                };
            var date = Assert.IsType<DateTimeOffset>(handler.Parse("2000-01-01-01:00"));
            Assert.Equal("2000-01-01 00:00", date.ToString("yyyy-MM-dd HH:mm"));
            Assert.Equal("2000-01-01-01:00", handler.Format(date));
        }

        [Fact]
        public void TestInvalidDate1()
        {
            var handler = new XmlDateTypeHandler();
            Assert.Throws<TypeConversionException>(() => handler.Parse("2000-01-01T10:00:00"));
        }

        [Fact]
        public void TestInvalidDate2()
        {
            var handler = new XmlDateTypeHandler();
            Assert.Throws<TypeConversionException>(() => handler.Parse("2000-02-30"));
        }
    }
}
