// <copyright file="FixedLengthParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;

using Xunit;

namespace BeanIO.Parser.FixedLength
{
    public class FixedLengthParserTest : ParserTest
    {
        [Fact]
        public void TestFieldDefinitions()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var reader = factory.CreateReader("f1", LoadReader("f1_valid.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(" value", map["default"]);
                Assert.Equal(12345, map["number"]);
                Assert.Equal("value", map["padx"]);
                Assert.Equal("value", map["pos40"]);

                var text = new StringWriter();
                var writer = factory.CreateWriter("f1", text);
                writer.Write(map);
                writer.Flush();
                writer.Close();
                Assert.Equal(" value    0000012345valuexxxxx          value", text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestDefaultMinLengthValidation()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var reader = factory.CreateReader("f1", LoadReader("f1_minLength.txt"));
            try
            {
                Assert.ThrowsAny<InvalidRecordException>(() => reader.Read());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestDefaultMaxLengthValidation()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var reader = factory.CreateReader("f1", LoadReader("f1_maxLength.txt"));
            try
            {
                Assert.ThrowsAny<InvalidRecordException>(() => reader.Read());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestOptionalField()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var reader = factory.CreateReader("f2", LoadReader("f2_valid.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("field3"));
                Assert.Equal("value", map["field3"]);

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("field3"));
                Assert.Equal("value", map["field3"]);

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.False(map.ContainsKey("field3"));

                var text = new StringWriter();
                factory.CreateWriter("f2", text).Write(map);
                Assert.Equal("1234512345\r\n", text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestValidation()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var reader = factory.CreateReader("f2", LoadReader("f2_invalid.txt"));
            try
            {
                AssertRecordError(reader, 1, "record", "minLength, 1, Record Label, 12345, 10, 20");
                AssertRecordError(reader, 2, "record", "maxLength, 2, Record Label, 123456789012345678901, 10, 20");
                AssertFieldError(reader, 3, "record", "field3", "val", "Invalid field length, expected 5 characters");
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestReader()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var reader = factory.CreateReader("f3", LoadReader("f3_valid.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("field1"));
                Assert.Equal("00001", map["field1"]);
                Assert.True(map.ContainsKey("field2"));
                Assert.Equal(string.Empty, map["field2"]);
                Assert.True(map.ContainsKey("field3"));
                Assert.Equal("XXXXX", map["field3"]);

                var text = new StringWriter();
                factory.CreateWriter("f3", text).Write(map);
                Assert.Equal("00001     XXXXX" + LineSeparator, text.ToString());

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("field1"));
                Assert.Equal("00002", map["field1"]);
                Assert.True(map.ContainsKey("field2"));
                Assert.Equal("Val2", map["field2"]);

                map["field2"] = "Value2";

                text = new StringWriter();
                factory.CreateWriter("f3", text).Write(map);
                Assert.Equal("00002Value" + LineSeparator, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestPadding()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var reader = factory.CreateReader("f4", LoadReader("f4_padding.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("number"));
                Assert.Equal(new int?[] { 0, 1, 10, 100, 1000, 10000, null }, Assert.IsType<List<int?>>(map["number"]));

                var text = new StringWriter();
                factory.CreateWriter("f4", text).Write(map);
                Assert.Equal("INT000000000100010001000100010000     ", text.ToString());

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("character"));
                Assert.Equal(new[] { 'A', 'B', ' ', 'D' }, Assert.IsType<List<char>>(map["character"]));

                text = new StringWriter();
                factory.CreateWriter("f4", text).Write(map);
                Assert.Equal("CHAAB D", text.ToString());

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("stringLeft"));
                Assert.Equal(new[] { "TXT", "TX", "T", string.Empty }, Assert.IsType<List<string>>(map["stringLeft"]));

                text = new StringWriter();
                factory.CreateWriter("f4", text).Write(map);
                Assert.Equal("STLTXTTX T     ", text.ToString());

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("stringRight"));
                Assert.Equal(new[] { "TXT", "TX", "T", string.Empty }, Assert.IsType<List<string>>(map["stringRight"]));

                text = new StringWriter();
                factory.CreateWriter("f4", text).Write(map);
                Assert.Equal("STRTXT TX  T   ", text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestIgnoredField()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var reader = factory.CreateReader("f5", LoadReader("f5_valid.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("lastName"));
                Assert.Equal("Smith", map["lastName"]);
                Assert.False(map.ContainsKey("firstName"));

                var text = new StringWriter();
                factory.CreateWriter("f5", text).Write(map);
                Assert.Equal("AAAAAAAAAASmith     ", text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestVariableLengthField()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var u = factory.CreateUnmarshaller("f6");
            var record = "kevin     johnson";
            var map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal(record));
            Assert.True(map.ContainsKey("firstName"));
            Assert.Equal("kevin", map["firstName"]);
            Assert.True(map.ContainsKey("lastName"));
            Assert.Equal("johnson", map["lastName"]);

            var m = factory.CreateMarshaller("f6");
            Assert.Equal(record, m.Marshal(map).ToString());
        }

        [Fact]
        public void TestKeepPadding()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var m = factory.CreateMarshaller("f7");
            var reader = factory.CreateReader("f7", LoadReader("f7.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("firstName"));
                Assert.Equal("kevin     ", map["firstName"]);
                Assert.True(map.ContainsKey("lastName"));
                Assert.Equal("          ", map["lastName"]);
                Assert.Equal("kevin               ", m.Marshal(map).ToString());

                AssertFieldError(reader, 2, "record", "firstName", "          ", "Required field not set");
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestOverlay()
        {
            var factory = NewStreamFactory("BeanIO.Parser.FixedLength.fixedlength.xml");
            var output = new StringWriter();
            var writer = factory.CreateWriter("f8", output);

            var map = new System.Collections.Hashtable();
            map["number"] = 3;
            map["name"] = "LAUREN1";
            writer.Write("record1", map);

            map.Clear();
            map["number"] = 5;
            writer.Write("record2", map);

            writer.Flush();

            Assert.Equal("003LAUREN1\n0005\n", output.ToString());
        }
    }
}
