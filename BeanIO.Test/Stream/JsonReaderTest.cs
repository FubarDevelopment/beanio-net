using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BeanIO.Stream.Json;

using Newtonsoft.Json.Linq;

using Xunit;

namespace BeanIO.Stream
{
    /// <summary>
    /// test cases for <see cref="JsonReader"/> and <see cref="JsonRecordUnmarshaller"/>
    /// </summary>
    public class JsonReaderTest
    {

        [Fact]
        public void TestReadString()
        {
            var reader = NewReader(
                "{\"field1\":\"value1\"}\n" +
                "{ \"field2\" : \"value2\" }");

            var map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal("value1", (string)map["field1"]);
            map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal("value2", (string)map["field2"]);
            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestReadNumber()
        {
            var long1 = long.MaxValue;
            var int1 = int.MaxValue;

            var reader = NewReader(
                "{\"double1\":5e10}\n" +
                "{\"double2\":5.1}\n" +
                "{ \"double3\" : 5E10 }\n" +
                "{\"int1\":" + int1 + "}\n" +
                "{\"int2\":10}\n" +
                "{ \"long1\" : " + long1 + " }\n"
                );

            var map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(5e10d, map["double1"]);
            map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(5.1d, map["double2"]);
            map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(5E10d, map["double3"]);
            map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(int1, map["int1"]);
            map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(10, map["int2"]);
            map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(long1, map["long1"]);

            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestReadNull()
        {
            var reader = NewReader(
                "{\"field1\":null}\n" +
                "{ \"field2\" : null }\n"
                );

            var map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(JValue.CreateNull(), map["field1"]);
            map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(JValue.CreateNull(), map["field2"]);

            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestReadBoolean()
        {
            var reader = NewReader(
                "{\"field1\":true}\n" +
                "{ \"field2\" : false }\n"
                );

            var map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(true, map["field1"]);
            map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(false, map["field2"]);

            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestReadObject()
        {
            var reader = NewReader(
                "{\"o1\":{\"field1\":\"value1\"}, \"field2\":20}" +
                "{ \"o1\" : { \"field1\" : \"value1\" } }" +
                "{ \"o1\" : { \"field1\" : \"value1\", \"field2\" : 10} }"
                );

            var map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(20, map["field2"]);
            var o = Assert.IsType<JObject>(map["o1"]);
            Assert.Equal("value1", o["field1"]);

            map = Assert.IsType<JObject>(reader.Read());
            o = Assert.IsType<JObject>(map["o1"]);
            Assert.Equal("value1", o["field1"]);

            map = Assert.IsType<JObject>(reader.Read());
            o = Assert.IsType<JObject>(map["o1"]);
            Assert.Equal("value1", o["field1"]);
            Assert.Equal(10, o["field2"]);

            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestReadArray()
        {
            var reader = NewReader(
                "{\"array1\":[1,2,3]}" +
                "{ \"array2\" : [ \"10\" , null , true , { \"field1\" : \"value1\" } ] }"
                );

            var map = Assert.IsType<JObject>(reader.Read());
            var list = Assert.IsType<JArray>(map["array1"]);
            Assert.Equal(new[] { new JValue(1), new JValue(2), new JValue(3) }, list);

            map = Assert.IsType<JObject>(reader.Read());
            list = Assert.IsType<JArray>(map["array2"]);
            Assert.Collection(
                list,
                item => Assert.Equal("10", item),
                item => Assert.Equal(JValue.CreateNull(), item),
                item => Assert.Equal(true, item),
                item =>
                    {
                        var o = Assert.IsType<JObject>(item);
                        Assert.Equal("value1", o["field1"]);
                    });

            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestEscapedString()
        {
            var reader = NewReader(
                "{ \"field1\" : \" \\\\ \\/ \\b \\f \\n \\r \\t \\\" \\u004B \" } "
                );

            var map = Assert.IsType<JObject>(reader.Read());
            Assert.Equal(" \\ / \b \f \n \r \t \" K ", map["field1"]);

            Assert.Null(reader.Read());
        }

        [Fact]
        public void TestMixedArray()
        {
            JsonRecordUnmarshaller u = new JsonRecordUnmarshaller();

            var map = Assert.IsType<JObject>(
                u.Unmarshal("{ \"array\" : [ [10], { \"field\":\"value\" } ]}"));

            var array = Assert.IsType<JArray>(map["array"]);
            Assert.Collection(
                array,
                item =>
                    {
                        var list = Assert.IsType<JArray>(item);
                        Assert.Collection(
                            list,
                            listItem => Assert.Equal(10, listItem));
                    },
                item =>
                    {
                        var subMap = Assert.IsType<JObject>(item);
                        Assert.Equal("value", subMap["field"]);
                    });
        }

        [Fact]
        public void TestMissingObject()
        {
            Assert.Null(new JsonRecordUnmarshaller().Unmarshal(null));
            Assert.Null(new JsonRecordUnmarshaller().Unmarshal(string.Empty));
            Assert.Null(new JsonRecordUnmarshaller().Unmarshal(" "));
            AssertError("1", "Unexpected token Integer at line 1, near position 1");
        }

        [Fact]
        public void TestMissingCommaInObject()
        {
            AssertError("{ \"f1\" : \"value\" \"f2\" : \"value2\" }", "After parsing a value an unexpected character was encountered: \". Path 'f1', line 1, position 17.");
        }

        [Fact]
        public void TestMissingCommaInArray()
        {
            AssertError("{ \"array\" : [ 10 20 ] }", "After parsing a value an unexpected character was encountered: 2. Path 'array[0]', line 1, position 17.");
        }

        [Fact]
        public void TestInvalidValue()
        {
            AssertError("{ \"number\" : a }", "Unexpected character encountered while parsing value: a. Path 'number', line 1, position 13.");
        }

        [Fact]
        public void TestMissingCloseObject()
        {
            AssertError("{ \"number\" : 10", "Unexpected end of content while loading JObject. Path 'number', line 1, position 15.");
        }

        [Fact]
        public void TestMissingCloseArray()
        {
            AssertError("{ \"number\" : [ 10", "Unexpected end of content while loading JObject. Path 'number[0]', line 1, position 17.");
        }

        [Fact]
        public void TestMissingCloseString()
        {
            AssertError("{ \"number", "Unterminated string. Expected delimiter: \". Path '', line 1, position 9.");
        }

        [Fact]
        public void TestMissingColon()
        {
            AssertError("{ \"number\" 10 }", "Invalid character after parsing property name. Expected ':' but got: 1. Path '', line 1, position 11.");
        }

        private JsonReader NewReader(string text)
        {
            return new JsonReader(new StringReader(text));
        }

        private void AssertError(string record, [JetBrains.Annotations.UsedImplicitly] string message)
        {
            var ex = Assert.ThrowsAny<RecordIOException>(() => new JsonRecordUnmarshaller().Unmarshal(record));
            Assert.Equal(message, ex.Message);
        }
    }
}
