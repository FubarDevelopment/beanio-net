using System;
using System.Collections.Generic;
using System.IO;

using NodaTime;

using Xunit;

namespace BeanIO.Parser.Types
{
    public class TypesParserTest : ParserTest
    {
        [Fact]
        public void TestObjectHandlers()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Types.types.xml");
            var reader = factory.CreateReader("t1", LoadStream("t1_valid.txt"));
            try
            {
                var record = Assert.IsType<ObjectRecord>(reader.Read());
                Assert.Equal((byte)10, record.ByteValue);
                Assert.Equal((short)10, record.ShortValue);
                Assert.Equal(-10, record.IntegerValue);
                Assert.Equal(10, record.LongValue);
                Assert.Equal(10.1f, record.FloatValue);
                Assert.Equal(-10.1, record.DoubleValue);
                Assert.Equal('A', record.CharacterValue);
                Assert.Equal("ABC", record.StringValue);
                Assert.Equal(new LocalDate(1970, 1, 1), record.DateValue);
                Assert.True(record.BooleanValue);
                Assert.Equal(10m, record.DecimalValue);
                Assert.Equal(new Guid("fbd9d2be-35dc-41fb-abc9-f4b4c8757eb5"), record.Id);
                Assert.Equal(new Uri("http://www.google.com"), record.Url);
                Assert.Equal(TypeEnum.ONE, record.Enum1);
                Assert.Equal(TypeEnum.TWO, record.Enum2);

                var text = new StringWriter();
                factory.CreateWriter("t1", text).Write(record);
                Assert.Equal(
                    "10,10,-10,10,10.1,-10.1,A,ABC,010170,True,10," +
                    "fbd9d2be-35dc-41fb-abc9-f4b4c8757eb5,http://www.google.com/,ONE,two" + LineSeparator,
                    text.ToString());

                record = Assert.IsType<ObjectRecord>(reader.Read());
                Assert.Null(record.ByteValue);
                Assert.Null(record.ShortValue);
                Assert.Null(record.IntegerValue);
                Assert.Null(record.LongValue);
                Assert.Null(record.FloatValue);
                Assert.Null(record.DoubleValue);
                Assert.Null(record.CharacterValue);
                Assert.Equal(string.Empty, record.StringValue);
                Assert.Null(record.DateValue);
                Assert.Null(record.BooleanValue);
                Assert.Null(record.DecimalValue);
                Assert.Null(record.Id);
                Assert.Null(record.Url);
                Assert.Null(record.Enum1);
                Assert.Null(record.Enum2);

                text = new StringWriter();
                factory.CreateWriter("t1", text).Write(record);
                Assert.Equal(
                    ",,,,,,,,,,,,,," + LineSeparator,
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestPrimitiveHandlers()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Types.types.xml");
            var reader = factory.CreateReader("t2", LoadStream("t2_valid.txt"));
            try
            {
                var record = Assert.IsType<PrimitiveRecord>(reader.Read());
                Assert.Equal((byte)10, record.ByteValue);
                Assert.Equal((short)10, record.ShortValue);
                Assert.Equal(-10, record.IntegerValue);
                Assert.Equal(10, record.LongValue);
                Assert.Equal(10.1f, record.FloatValue);
                Assert.Equal(-10.1, record.DoubleValue);
                Assert.Equal('A', record.CharacterValue);
                Assert.True(record.BooleanValue);
                Assert.Equal(10.1m, record.DecimalValue);

                var text = new StringWriter();
                factory.CreateWriter("t2", text).Write(record);
                Assert.Equal(
                    "10,10,-10,10,10.1,-10.1,A,True,10.1" + LineSeparator,
                    text.ToString());

                record = Assert.IsType<PrimitiveRecord>(reader.Read());
                Assert.Equal((byte)0, record.ByteValue);
                Assert.Equal((short)0, record.ShortValue);
                Assert.Equal(0, record.IntegerValue);
                Assert.Equal(0, record.LongValue);
                Assert.Equal(0, record.FloatValue);
                Assert.Equal(0, record.DoubleValue);
                Assert.Equal('x', record.CharacterValue);
                Assert.False(record.BooleanValue);
                Assert.Equal(0, record.DecimalValue);

                text = new StringWriter();
                factory.CreateWriter("t2", text).Write(record);
                Assert.Equal(
                    "0,0,0,0,0,0,x,False,0" + LineSeparator,
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestStreamTypeHandler()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Types.types.xml");
            var reader = factory.CreateReader("t3", LoadStream("t3_valid.txt"));
            try
            {
                var record = Assert.IsType<ObjectRecord>(reader.Read());
                Assert.Equal(new LocalDate(1970, 1, 1), record.DateValue);

                var text = new StringWriter();
                factory.CreateWriter("t3", text).Write(record);
                Assert.Equal(
                    "01-01-1970" + LineSeparator,
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestNamedTypeHandler()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Types.types.xml");
            var reader = factory.CreateReader("t4", LoadStream("t4_valid.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(new LocalTime(12, 0), map["dateValue"]);

                var text = new StringWriter();
                factory.CreateWriter("t4", text).Write(map);
                Assert.Equal(
                    "12:00:00" + LineSeparator,
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestStringTypeHandler()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Types.types.xml");
            var reader = factory.CreateReader("t5", LoadStream("t5_valid.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(string.Empty, map["field"]);

                var text = new StringWriter();
                factory.CreateWriter("t5", text).Write(map);
                Assert.Equal(
                    string.Empty + LineSeparator,
                    text.ToString());

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal("Text", map["field"]);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestNullPrimitive()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Types.types.xml");
            var reader = factory.CreateReader("t6", new StringReader("\n"));
            var record = Assert.IsType<PrimitiveRecord>(reader.Read());
            Assert.Equal(0, record.IntegerValue);
        }

        [Fact(Skip = "There is no ParseExact for numeric types")]
        public void TestFormats()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Types.types.xml");
            var reader = factory.CreateReader("t7", LoadStream("t7_valid.txt"));
            try
            {
                var record = Assert.IsType<ObjectRecord>(reader.Read());
                Assert.Equal((byte)1, record.ByteValue);
                Assert.Equal((short)2, record.ShortValue);
                Assert.Equal(-3, record.IntegerValue);
                Assert.Equal(4, record.LongValue);
                Assert.Equal(5.1f, record.FloatValue);
                Assert.Equal(-6.1, record.DoubleValue);
                Assert.Equal(new LocalDate(2011, 1, 1), record.DateValue);
                Assert.Equal(10.5m, record.DecimalValue);

                var text = new StringWriter();
                factory.CreateWriter("t7", text).Write(record);
                Assert.Equal(
                    "1x,2x,-3x,4x,5.10x,-6.10x,2011-01-01,10.50x" + LineSeparator,
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestFormatSpecificTypeHandler()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Types.types.xml");
            var reader = factory.CreateReader("t8", LoadStream("t8_valid.txt"));
            try
            {
                var record = Assert.IsType<ObjectRecord>(reader.Read());
                Assert.Equal(new LocalDate(2000, 1, 1), record.DateValue);

                var text = new StringWriter();
                factory.CreateWriter("t8", text).Write(record);
                Assert.Equal(
                    "2000-01-01" + LineSeparator,
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.Types.{0}", fileName);
            var asm = typeof(TypesParserTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            if (resStream == null)
                throw new ArgumentOutOfRangeException("fileName");
            return new StreamReader(resStream);
        }
    }
}
