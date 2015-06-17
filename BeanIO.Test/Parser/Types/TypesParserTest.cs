using System;
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
