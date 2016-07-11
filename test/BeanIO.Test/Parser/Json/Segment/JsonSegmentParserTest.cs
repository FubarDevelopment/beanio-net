// <copyright file="JsonSegmentParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;

using BeanIO.Beans;

using Xunit;

namespace BeanIO.Parser.Json.Segment
{
    public sealed class JsonSegmentParserTest : ParserTest
    {
        private readonly StreamFactory _factory;

        public JsonSegmentParserTest()
        {
            _factory = NewStreamFactory("jsonSegment_mapping.xml");
        }

        [Fact]
        public void TestSegmentJsonTypeObject()
        {
            var reader = _factory.CreateReader("stream1", LoadReader("js1.txt"));

            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal("1234", map["account"]);

                var person = Assert.IsType<Person>(map["customer"]);
                Assert.Equal("Jen", person.FirstName);
                Assert.Equal("Jones", person.LastName);

                var text = new StringWriter();
                _factory.CreateWriter("stream1", text).Write(map);
                Assert.Equal(Load("js1.txt"), text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestSegmentJsonTypeObjectList()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("js2.txt"));

            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(2, map["count"]);

                var list = Assert.IsType<List<Person>>(map["friends"]);
                Assert.Collection(
                    list,
                    person =>
                        {
                            Assert.Equal("Jen", person.FirstName);
                            Assert.Equal("Jones", person.LastName);
                        },
                    person =>
                        {
                            Assert.Equal("Mary", person.FirstName);
                            Assert.Equal("Smith", person.LastName);
                        });

                var text = new StringWriter();
                _factory.CreateWriter("stream2", text).Write(map);
                Assert.Equal(Load("js2.txt"), text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestSegmentJsonTypeArray()
        {
            var reader = _factory.CreateReader("stream3", LoadReader("js3.txt"));

            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(new[] { 1, 2, 3 }, Assert.IsType<List<int>>(map["numbers"]));

                var person = Assert.IsType<Person>(map["person"]);
                Assert.Equal("Jen", person.FirstName);
                Assert.Equal("Jones", person.LastName);

                var text = new StringWriter();
                _factory.CreateWriter("stream3", text).Write(map);
                Assert.Equal(Load("js3.txt"), text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestSegmentJsonTypeNone()
        {
            var reader = _factory.CreateReader("stream4", LoadReader("js4.txt"));

            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());

                var person = Assert.IsType<Person>(map["person"]);
                Assert.Equal("Jen", person.FirstName);
                Assert.Equal("Jones", person.LastName);
                Assert.Equal("1234", map["account"]);

                var text = new StringWriter();
                var writer = _factory.CreateWriter("stream4", text);
                writer.Write(map);

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                person = Assert.IsType<Person>(map["person"]);
                Assert.Equal("Jason", person.FirstName);
                Assert.Equal("Jones", person.LastName);
                Assert.Equal("5678", map["account"]);
                writer.Write(map);

                Assert.Equal(Load("js4.txt"), text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
