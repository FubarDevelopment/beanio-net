using System;
using System.Collections.Generic;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Misc
{
    public class MiscParserTest : ParserTest
    {
        [Fact]
        public void TestRecordWithoutFieldsClassNotSet()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Misc.misc_mapping.xml");
            var reader = factory.CreateReader("stream1", LoadReader("m1.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("field1"));
                Assert.Equal("Joe", map["field1"]);

                var text = new StringWriter();
                factory.CreateWriter("stream1", text).Write(map);
                Assert.Equal("Joe,Smith" + LineSeparator, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestRecordWithoutFieldsClassSet()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Misc.misc_mapping.xml");
            var reader = factory.CreateReader("stream2", LoadReader("m1.txt"));
            try
            {
                var text = new StringWriter();
                var writer = factory.CreateWriter("stream2", text);

                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(0, map.Count);
                writer.Write(map);

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("field1"));
                Assert.Equal("Joe", map["field1"]);
                writer.Write(map);

                Assert.Equal(
                    LineSeparator +
                    "Joe,Smith" + LineSeparator,
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestRecordWithPropertyOnlyClassSet()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Misc.misc_mapping.xml");
            var reader = factory.CreateReader("stream3", LoadReader("m1.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("key"));
                Assert.Equal("value", map["key"]);

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("field1"));
                Assert.Equal("Joe", map["field1"]);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestTypeValidationClassNotSet()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Misc.misc_mapping.xml");
            var reader = factory.CreateReader("stream4", LoadReader("m1.txt"));
            try
            {
                AssertFieldError(reader, 1, "header", "field1", "FirstName", "Type conversion error: Invalid date");

                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("field1"));
                Assert.Equal("Joe", map["field1"]);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestRecordWithPropertyOnlyClassNotSet()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Misc.misc_mapping.xml");
            var reader = factory.CreateReader("stream5", LoadReader("m1.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.False(map.ContainsKey("bean1"));

                Assert.True(map.ContainsKey("bean"));
                map = Assert.IsType<Dictionary<string, object>>(map["bean"]);
                Assert.Equal("value", map["key"]);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestMarshalStaticRecord()
        {
            var text = new StringWriter();
            var factory = NewStreamFactory("BeanIO.Parser.Misc.misc_mapping.xml");

            var writer = factory.CreateWriter("stream6", text);
            writer.Write("header", null);

            var map = new Dictionary<object, object>
                {
                    { "d1", "value1" },
                };
            writer.Write(map);
            writer.Flush();

            Assert.Equal(
                "Header1,Header2,Header3" + LineSeparator +
                "value1,," + LineSeparator,
                text.ToString());

            var m = factory.CreateMarshaller("stream6");
            Assert.Equal("Header1,Header2,Header3", m.Marshal("header", null).ToString());
        }
    }
}
