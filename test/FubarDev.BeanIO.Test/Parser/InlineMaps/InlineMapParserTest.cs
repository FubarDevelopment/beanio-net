// <copyright file="InlineMapParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;

using BeanIO.Beans;

using Xunit;

namespace BeanIO.Parser.InlineMaps
{
    public class InlineMapParserTest : ParserTest
    {
        [Fact]
        public void TestMapWithClass()
        {
            var factory = NewStreamFactory("map_mapping.xml");
            var u = factory.CreateUnmarshaller("stream1");
            var m = factory.CreateMarshaller("stream1");

            var text = "js,Joe,Smith,bm,Bob,Marshall";
            var map = Assert.IsType<Dictionary<string, Person>>(u.Unmarshal(text));

            Assert.True(map.ContainsKey("js"));
            var person = map["js"];
            Assert.Equal("js", person.Id);
            Assert.Equal("Joe", person.FirstName);
            Assert.Equal("Smith", person.LastName);

            Assert.True(map.ContainsKey("bm"));
            person = map["bm"];
            Assert.Equal("bm", person.Id);
            Assert.Equal("Bob", person.FirstName);
            Assert.Equal("Marshall", person.LastName);

            Assert.Equal(text, m.Marshal(map).ToString());
        }

        [Fact]
        public void TestMapWithTarget()
        {
            var factory = NewStreamFactory("map_mapping.xml");
            var u = factory.CreateUnmarshaller("stream2");
            var m = factory.CreateMarshaller("stream2");

            var text = "js,Joe,Smith,bm,Bob,Marshall";
            var map = Assert.IsType<Dictionary<string, string>>(u.Unmarshal(text));

            Assert.True(map.ContainsKey("js"));
            Assert.Equal("Joe", map["js"]);

            Assert.True(map.ContainsKey("bm"));
            Assert.Equal("Bob", map["bm"]);

            Assert.Equal("js,Joe,,bm,Bob,", m.Marshal(map).ToString());
        }

        [Fact]
        public void TestRecordBoundMap()
        {
            var factory = NewStreamFactory("map_mapping.xml");
            var u = factory.CreateUnmarshaller("stream3");
            var m = factory.CreateMarshaller("stream3");

            var text = "J,1,key1,value1,key2,value2";
            var job = Assert.IsType<Job>(u.Unmarshal(text));

            Assert.Equal("1", job.Id);

            var map = Assert.IsType<Dictionary<string, string>>(job.Codes);
            Assert.Equal(2, map.Count);
            Assert.True(map.ContainsKey("key1"));
            Assert.Equal("value1", map["key1"]);
            Assert.True(map.ContainsKey("key2"));
            Assert.Equal("value2", map["key2"]);

            Assert.Equal(text, m.Marshal(job).ToString());
        }

        [Fact]
        public void TestGroupBoundMap()
        {
            var factory = NewStreamFactory("map_mapping.xml");
            var text = "key1,value1\nkey2,value2";
            var reader = factory.CreateReader("stream4", new StringReader(text));
            var job = Assert.IsType<Job>(reader.Read());
            var map = Assert.IsType<Dictionary<string, string>>(job.Codes);
            Assert.True(map.ContainsKey("key1"));
            Assert.Equal("value1", map["key1"]);
            Assert.True(map.ContainsKey("key2"));
            Assert.Equal("value2", map["key2"]);
        }

        [Fact]
        public void TestMapRecordGroup()
        {
            var text =
                "entity,PERSON,8.400000,-77.200000,TEST_ENTITY_1\n" +
                "detail,foo,bar\n" +
                "detail,foo2,bar2\n" +
                "entity,PERSON,-33.993670,25.676320,TEST_ENTITY_2\n" +
                "entity,PERSON,-22.282174,166.441458,TEST_ENTITY_3\n";

            var factory = NewStreamFactory("map_mapping.xml");
            var reader = factory.CreateReader("stream5", new StringReader(text));
            var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
            Assert.True(map.ContainsKey("name"));
            Assert.Equal("TEST_ENTITY_1", map["name"]);
            Assert.True(map.ContainsKey("details"));
            var details = Assert.IsType<Dictionary<string, string>>(map["details"]);
            Assert.Equal(2, details.Count);
            Assert.True(details.ContainsKey("foo"));
            Assert.Equal("bar", details["foo"]);
            Assert.True(details.ContainsKey("foo2"));
            Assert.Equal("bar2", details["foo2"]);
            map = Assert.IsType<Dictionary<string, object>>(reader.Read());
            Assert.True(map.ContainsKey("name"));
            Assert.Equal("TEST_ENTITY_2", map["name"]);
            Assert.False(map.ContainsKey("details"));
            map = Assert.IsType<Dictionary<string, object>>(reader.Read());
            Assert.True(map.ContainsKey("name"));
            Assert.Equal("TEST_ENTITY_3", map["name"]);
            Assert.False(map.ContainsKey("details"));
            Assert.Null(reader.Read());
        }
    }
}
