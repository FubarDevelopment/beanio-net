using System;
using System.Xml;

using BeanIO.Types.Xml;

using NodaTime;
using NodaTime.Text;

using Xunit;

namespace BeanIO.Types
{
    public class XmlTimeTypeHandlerTest
    {
        [Fact]
        public void TestTime()
        {
            var handler = new XmlTimeTypeHandler();
            var time = Assert.IsType<LocalTime>(handler.Parse("15:14:13"));
            Assert.Equal(new LocalTime(15, 14, 13), time);
            Assert.Equal("15:14:13", handler.Format(time));
        }

        [Fact]
        public void TestTimeWithMilliseconds()
        {
            var handler = new XmlTimeTypeHandler()
                {
                    OutputMilliseconds = true,
                };
            var time = Assert.IsType<LocalTime>(handler.Parse("15:14:13.1236"));
            Assert.Equal(LocalTimePattern.Create("HH:mm:ss.ffff", handler.Culture).Parse("15:14:13.1236").GetValueOrThrow(), time);
            Assert.Equal("15:14:13.123", handler.Format(time));
        }

        [Fact]
        public void TestTimeWithTimezone()
        {
            var handler = new XmlDateTimeOffsetTypeHandler()
                {
                    TimeZoneId = "GMT+1",
                    Pattern = "HH:mm:sszzzzzz",
                };
            var test = XmlConvert.ToDateTimeOffset("23:04:03+01:00", "HH:mm:sszzzzzz");
            test = new DateTimeOffset(new DateTime(1970, 1, 1) + test.TimeOfDay, test.Offset);
            var date = Assert.IsType<DateTimeOffset>(handler.Parse("23:04:03+01:00"));
            Assert.Equal(test, date);
            Assert.Equal("23:04:03+01:00", handler.Format(date));
        }

        [Fact]
        public void TestInvalidTime()
        {
            var handler = new XmlTimeTypeHandler();
            Assert.Throws<TypeConversionException>(() => handler.Parse("23:62:03+01:00"));
        }

        [Fact]
        public void TestInvalidTimeWithTimezone()
        {
            var handler = new XmlDateTimeOffsetTypeHandler()
            {
                IsTimeZoneAllowed = false,
                Pattern = "HH:mm:sszzzzzz",
            };
            Assert.Throws<TypeConversionException>(() => handler.Parse("23:04:03+01:00"));
        }
    }
}
