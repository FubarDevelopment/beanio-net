// <copyright file="IndeterminateSegmentsTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using Xunit;

namespace BeanIO.Parser.Indeterminates
{
    public class IndeterminateSegmentsTest : ParserTest
    {
        [Fact]
        public void TestDelimitedIndeterminateFieldBeforeEOR()
        {
            var factory = NewStreamFactory("indeterminates_mapping.xml");
            TestDelimitedIndeterminateFieldBeforeEORInternal(factory, "d1");
            TestDelimitedIndeterminateFieldBeforeEORInternal(factory, "d3");
        }

        [Fact]
        public void TestDelimitedIndeterminateSegmentBeforeEOR()
        {
            var factory = NewStreamFactory("indeterminates_mapping.xml");
            var text = "v1,v2.1,v3.1,v2.2,v3.2,v4,v5.1,v6.1,v6.2,v5.2,v6.3,v6.4,v7";
            var u = factory.CreateUnmarshaller("d2");
            var map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal(text));
            Assert.True(map.ContainsKey("f1"));
            Assert.Equal("v1", map["f1"]);

            var list = Assert.IsType<List<Dictionary<string, object>>>(map["rs1"]);
            Assert.Collection(
                list,
                subMap =>
                    {
                        Assert.True(subMap.ContainsKey("f2"));
                        Assert.Equal("v2.1", subMap["f2"]);
                        Assert.True(subMap.ContainsKey("f3"));
                        Assert.Equal("v3.1", subMap["f3"]);
                    },
                subMap =>
                    {
                        Assert.True(subMap.ContainsKey("f2"));
                        Assert.Equal("v2.2", subMap["f2"]);
                        Assert.True(subMap.ContainsKey("f3"));
                        Assert.Equal("v3.2", subMap["f3"]);
                    });
            Assert.True(map.ContainsKey("f4"));
            Assert.Equal("v4", map["f4"]);

            list = Assert.IsType<List<Dictionary<string, object>>>(map["rs2"]);
            Assert.Collection(
                list,
                subMap =>
                {
                    Assert.True(subMap.ContainsKey("f5"));
                    Assert.Equal("v5.1", subMap["f5"]);
                    Assert.True(subMap.ContainsKey("f6"));
                    var subList = Assert.IsType<List<string>>(subMap["f6"]);
                    Assert.Collection(
                        subList,
                        subItem => Assert.Equal("v6.1", subItem),
                        subItem => Assert.Equal("v6.2", subItem));
                },
                subMap =>
                {
                    Assert.True(subMap.ContainsKey("f5"));
                    Assert.Equal("v5.2", subMap["f5"]);
                    Assert.True(subMap.ContainsKey("f6"));
                    var subList = Assert.IsType<List<string>>(subMap["f6"]);
                    Assert.Collection(
                        subList,
                        subItem => Assert.Equal("v6.3", subItem),
                        subItem => Assert.Equal("v6.4", subItem));
                });
            Assert.True(map.ContainsKey("f7"));
            Assert.Equal("v7", map["f7"]);

            var m = factory.CreateMarshaller("d2");
            Assert.Equal(text, m.Marshal(map).ToString());
        }

        [Fact]
        public void TestNestedSegment()
        {
            var factory = NewStreamFactory("indeterminates_mapping.xml");
            var text = "v1,v2.1,v3.1,v2.2,v3.2";

            var u = factory.CreateUnmarshaller("d4");
            var map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal(text));
            Assert.True(map.ContainsKey("f1"));
            Assert.Equal("v1", map["f1"]);
            Assert.True(map.ContainsKey("rs1"));
            var subMap = Assert.IsType<Dictionary<string, object>>(map["rs1"]);
            Assert.True(subMap.ContainsKey("rs2"));

            var list = Assert.IsType<List<Dictionary<string, object>>>(subMap["rs2"]);
            Assert.Collection(
                list,
                itemMap =>
                    {
                        Assert.True(itemMap.ContainsKey("f2"));
                        Assert.Equal("v2.1", itemMap["f2"]);
                        Assert.True(itemMap.ContainsKey("f3"));
                        Assert.Equal("v3.1", itemMap["f3"]);
                    },
                itemMap =>
                    {
                        Assert.True(itemMap.ContainsKey("f2"));
                        Assert.Equal("v2.2", itemMap["f2"]);
                        Assert.True(itemMap.ContainsKey("f3"));
                        Assert.Equal("v3.2", itemMap["f3"]);
                    });

            var m = factory.CreateMarshaller("d4");
            Assert.Equal(text, m.Marshal(map).ToString());
        }

        [Fact]
        public void TestFixedLengthIndeterminateFieldBeforeEOR()
        {
            var text = "v1v2.1v2.2v2.3v3.1v3.2v4";
            var factory = NewStreamFactory("indeterminates_mapping.xml");

            var u = factory.CreateUnmarshaller("fl1");
            var map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal(text));
            Assert.Equal(4, map.Count);
            Assert.True(map.ContainsKey("f1"));
            Assert.Equal("v1", map["f1"]);
            Assert.True(map.ContainsKey("f2"));
            var list = Assert.IsType<List<string>>(map["f2"]);
            Assert.Collection(
                list,
                item => Assert.Equal("v2.1", item),
                item => Assert.Equal("v2.2", item),
                item => Assert.Equal("v2.3", item));
            Assert.True(map.ContainsKey("f3"));
            list = Assert.IsType<List<string>>(map["f3"]);
            Assert.Collection(
                list,
                item => Assert.Equal("v3.1", item),
                item => Assert.Equal("v3.2", item));
            Assert.True(map.ContainsKey("f4"));
            Assert.Equal("v4", map["f4"]);

            var m = factory.CreateMarshaller("fl1");
            Assert.Equal(text, m.Marshal(map).ToString());
        }

        private void TestDelimitedIndeterminateFieldBeforeEORInternal(StreamFactory factory, string stream)
        {
            var text = "v1,v2.1,v2.2,v3.1,v3.2,v4";
            var u = factory.CreateUnmarshaller(stream);

            var map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal(text));
            Assert.Equal(4, map.Count);
            Assert.True(map.ContainsKey("f1"));
            Assert.Equal("v1", map["f1"]);
            Assert.True(map.ContainsKey("f2"));
            var list = Assert.IsType<List<string>>(map["f2"]);
            Assert.Equal(new[] { "v2.1", "v2.2" }, list);
            Assert.True(map.ContainsKey("f3"));
            list = Assert.IsType<List<string>>(map["f3"]);
            Assert.Equal(new[] { "v3.1", "v3.2" }, list);
            Assert.True(map.ContainsKey("f4"));
            Assert.Equal("v4", map["f4"]);

            var m = factory.CreateMarshaller(stream);
            Assert.Equal(text, m.Marshal(map).ToString());
        }
    }
}
