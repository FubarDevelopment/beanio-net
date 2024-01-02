// <copyright file="DynamicOccursParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using BeanIO.Beans;

using Xunit;

namespace BeanIO.Parser.DynamicOccurs
{
    public class DynamicOccursParserTest : ParserTest
    {
        [Fact]
        public void TestDynamicFieldOccurrences()
        {
            var factory = NewStreamFactory("dynamicOccurs_mapping.xml");
            var u = factory.CreateUnmarshaller("o1");
            var m = factory.CreateMarshaller("o1");

            var text = "2,one,two,0,done";
            var map = (IDictionary?)u.Unmarshal(text);
            Assert.NotNull(map);
            Assert.False(map.Contains("occurs"));
            Assert.True(map.Contains("values"));
            var list = (IList?)map["values"];
            Assert.NotNull(list);
            Assert.Equal(new[] { "one", "two" }, list.Cast<string>());
            Assert.True(map.Contains("occurs2"));
            Assert.Equal(0, map["occurs2"]);
            Assert.True(map.Contains("values2"));
            list = (IList?)map["values2"];
            Assert.NotNull(list);
            Assert.Empty(list);
            Assert.True(map.Contains("after"));
            Assert.Equal("done", map["after"]);
            Assert.Equal(text, m.Marshal(map).ToString());

            text = "0,1,one,done";
            map = (IDictionary?)u.Unmarshal(text);
            Assert.NotNull(map);
            Assert.True(map.Contains("values"));
            list = (IList?)map["values"];
            Assert.NotNull(list);
            Assert.Empty(list);
            Assert.True(map.Contains("occurs2"));
            Assert.Equal(1, map["occurs2"]);
            list = (IList?)map["values2"];
            Assert.NotNull(list);
            Assert.Equal(new[] { "one" }, list.Cast<string>());
            Assert.True(map.Contains("after"));
            Assert.Equal("done", map["after"]);
            Assert.Equal(text, m.Marshal(map).ToString());

            map["occurs2"] = 0;
            Assert.Equal("0,0,done", m.Marshal(map).ToString());
        }

        [Fact]
        public void TestDynamicSegmentOccurrences()
        {
            var factory = NewStreamFactory("dynamicOccurs_mapping.xml");
            var u = factory.CreateUnmarshaller("o2");
            var m = factory.CreateMarshaller("o2");

            var text = "02Rob 00Mike020102end";
            var map = (Dictionary<string, object?>?)u.Unmarshal(text);
            Assert.NotNull(map);
            Assert.True(map.ContainsKey("people"));
            var people = (List<Person>?)map["people"];
            Assert.NotNull(people);
            Assert.Collection(
                people,
                person =>
                    {
                        Assert.Equal("Rob", person.FirstName);
                        Assert.NotNull(person.Numbers);
                        Assert.Empty(person.Numbers);
                    },
                person =>
                    {
                        Assert.Equal("Mike", person.FirstName);
                        Assert.Equal(new int?[] { 1, 2 }, person.Numbers);
                    });
            Assert.Equal(text, m.Marshal(map).ToString());

            text = "00end";
            map = (Dictionary<string, object?>?)u.Unmarshal(text);
            Assert.NotNull(map);
            Assert.True(map.ContainsKey("people"));
            people = (List<Person>?)map["people"];
            Assert.NotNull(people);
            Assert.Empty(people);
            Assert.Equal(text, m.Marshal(map).ToString());
        }

        [Fact]
        public void TestInlineMapDynamicOccurrences()
        {
            var factory = NewStreamFactory("dynamicOccurs_mapping.xml");
            var u = factory.CreateUnmarshaller("o3");
            var m = factory.CreateMarshaller("o3");

            var text = "0201Rob 02Mikeend";
            var map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal(text));
            Assert.NotNull(map);
            Assert.True(map.ContainsKey("names"));
            var inline = Assert.IsType<Dictionary<int, string>>(map["names"]);
            Assert.NotNull(inline);
            Assert.Collection(
                inline,
                item =>
                    {
                        Assert.Equal(1, item.Key);
                        Assert.Equal("Rob", item.Value);
                    },
                item =>
                    {
                        Assert.Equal(2, item.Key);
                        Assert.Equal("Mike", item.Value);
                    });
            Assert.Equal(text, m.Marshal(map).ToString());

            text = "00end";
            map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal(text));
            inline = Assert.IsType<Dictionary<int, string>>(map["names"]);
            Assert.NotNull(inline);
            Assert.Empty(inline);
            Assert.Equal(text, m.Marshal(map).ToString());
        }

        [Fact]
        public void TestArrayDynamicOccurrences()
        {
            var factory = NewStreamFactory("dynamicOccurs_mapping.xml");
            var u = factory.CreateUnmarshaller("o4");
            var m = factory.CreateMarshaller("o4");

            var text = "3,3,2,1,end";
            var map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal(text));
            Assert.True(map.ContainsKey("numbers"));
            var numbers = Assert.IsType<int[]>(map["numbers"]);
            Assert.Equal(new[] { 3, 2, 1 }, numbers);
            Assert.Equal(text, m.Marshal(map).ToString());

            text = "0,end";
            map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal(text));
            Assert.True(map.ContainsKey("numbers"));
            numbers = Assert.IsType<int[]>(map["numbers"]);
            Assert.Equal(new int[0], numbers);
            Assert.Equal(text, m.Marshal(map).ToString());
        }

        [Fact]
        public void TestDynamicOccurrencesValidation()
        {
            var factory = NewStreamFactory("dynamicOccurs_mapping.xml");
            var u = factory.CreateUnmarshaller("o5");
            var m = factory.CreateMarshaller("o5");

            var text = "2,one,two";
            var list = Assert.IsType<List<string>>(u.Unmarshal(text));
            Assert.Equal(new[] { "one", "two" }, list);
            Assert.Equal(text, m.Marshal(list).ToString());

            var exception = Assert.ThrowsAny<InvalidRecordException>(() => u.Unmarshal("0"));
            Assert.NotNull(exception.RecordContext);
            Assert.Equal(new[] { "Expected minimum 1 occurrences" }, exception.RecordContext.GetFieldErrors("values").AsEnumerable());

            exception = Assert.ThrowsAny<InvalidRecordException>(() => u.Unmarshal("3,one,two,three"));
            Assert.NotNull(exception.RecordContext);
            Assert.Equal(new[] { "Expected maximum 2 occurrences" }, exception.RecordContext.GetFieldErrors("values").AsEnumerable());
        }

        [Fact]
        public void TestDynamicOccurrencesMultiple()
        {
            var factory = NewStreamFactory("dynamicOccurs_mapping.xml");
            var u = factory.CreateUnmarshaller("o6");
            var m = factory.CreateMarshaller("o6");

            var text = "10t1test      ";
            var record = Assert.IsType<TestRecord>(u.Unmarshal(text));
            Assert.NotNull(record.Item);
            Assert.Null(record.OtherItem);
            Assert.Equal(1, record.ItemCount);
            Assert.Equal(0, record.OtherItemCount);
            Assert.Equal("t1", record.Item.Id);
            Assert.Equal("test", record.Item.Text);
            Assert.Equal(text, m.Marshal(record).ToString());

            text = "01t2test      ";
            record = Assert.IsType<TestRecord>(u.Unmarshal(text));
            Assert.Null(record.Item);
            Assert.NotNull(record.OtherItem);
            Assert.Equal(0, record.ItemCount);
            Assert.Equal(1, record.OtherItemCount);
            Assert.Equal("t2", record.OtherItem.Id);
            Assert.Equal("test", record.OtherItem.Text);
            Assert.Equal(text, m.Marshal(record).ToString());
        }

        [Fact]
        public void TestDynamicOccurrencesMultipleWithBool()
        {
            var factory = NewStreamFactory("dynamicOccurs_mapping.xml");
            var u = factory.CreateUnmarshaller("o7");
            var m = factory.CreateMarshaller("o7");

            var text = "YNt1test      ";
            var record = Assert.IsType<TestRecordBool>(u.Unmarshal(text));
            Assert.True(record.HasItem);
            Assert.False(record.HasOtherItem);
            Assert.NotNull(record.Item);
            Assert.Null(record.OtherItem);
            Assert.Equal("t1", record.Item.Id);
            Assert.Equal("test", record.Item.Text);
            Assert.Equal(text, m.Marshal(record).ToString());

            text = "NYt2test      ";
            record = Assert.IsType<TestRecordBool>(u.Unmarshal(text));
            Assert.False(record.HasItem);
            Assert.True(record.HasOtherItem);
            Assert.Null(record.Item);
            Assert.NotNull(record.OtherItem);
            Assert.Equal("t2", record.OtherItem.Id);
            Assert.Equal("test", record.OtherItem.Text);
            Assert.Equal(text, m.Marshal(record).ToString());
        }
    }
}
