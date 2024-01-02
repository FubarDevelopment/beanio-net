// <copyright file="InlineMapsTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using BeanIO.Annotation;
using BeanIO.Builder;

using Xunit;

namespace BeanIO.Parser.InlineMaps
{
    public class InlineMapsTest : AbstractParserTest
    {
        [Fact]
        public void TestAnnotatedSegmentMap()
        {
            var factory = CreateFactory(@"
                <stream name=""s"" format=""csv"" strict=""true"">
                  <record name=""record"" class=""BeanIO.Parser.InlineMaps.InlineMapsTest+AnnotatedRecord, FubarDev.BeanIO.Test"" />
                </stream>");
            ValidateSegmentMap(factory);
        }

        [Fact]
        public void TestBuilderSegmentMap()
        {
            var factory = CreateFactory(
                new StreamBuilder("s")
                    .Format("csv")
                    .AddRecord(
                        new RecordBuilder("record")
                            .Type(typeof(AnnotatedRecord))
                            .AddSegment(
                                new SegmentBuilder("map")
                                    .Type(typeof(AnnotatedSegment))
                                    .Collection(typeof(Dictionary<string, object>))
                                    .Occurs(0, -1)
                                    .Key("key")
                                    .AddField(new FieldBuilder("key"))
                                    .AddField(new FieldBuilder("value")))));
            ValidateSegmentMap(factory);
        }

        [Fact]
        public void TestAnnotatedSegmentMapWithValue()
        {
            var factory = CreateFactory(@"
                <stream name=""s"" format=""csv"" strict=""true"">
                  <record name=""record"" class=""BeanIO.Parser.InlineMaps.InlineMapsTest+AnnotatedRecord2, FubarDev.BeanIO.Test"" />
                </stream>");
            ValidateSegmentMapWithValue(factory);
        }

        private void ValidateSegmentMap(StreamFactory factory)
        {
            var u = factory.CreateUnmarshaller("s");
            var m = factory.CreateMarshaller("s");

            var text = "key1,value1,key2,value2";
            var record = Assert.IsType<AnnotatedRecord>(u.Unmarshal(text));
            Assert.NotNull(record.Map);
            Assert.Collection(
                record.Map!,
                item =>
                    {
                        Assert.Equal("key1", item.Key);
                        Assert.Equal("key1", item.Value.Key);
                        Assert.Equal("value1", item.Value.Value);
                    },
                item =>
                    {
                        Assert.Equal("key2", item.Key);
                        Assert.Equal("key2", item.Value.Key);
                        Assert.Equal("value2", item.Value.Value);
                    });
            Assert.Equal(text, m.Marshal(record).ToString());
        }

        private void ValidateSegmentMapWithValue(StreamFactory factory)
        {
            var u = factory.CreateUnmarshaller("s");
            var m = factory.CreateMarshaller("s");

            var text = "key1,value1,key2,value2";
            var record = Assert.IsType<AnnotatedRecord2>(u.Unmarshal(text));
            Assert.NotNull(record.Map);
            Assert.Collection(
                record.Map!,
                item =>
                {
                    Assert.Equal("key1", item.Key);
                    Assert.Equal("key1", item.Value.Key);
                    Assert.Equal("value1", item.Value.Value);
                },
                item =>
                {
                    Assert.Equal("key2", item.Key);
                    Assert.Equal("key2", item.Value.Key);
                    Assert.Equal("value2", item.Value.Value);
                });
            Assert.Equal(text, m.Marshal(record).ToString());
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class AnnotatedSegment
        {
            [Field(At = 0)]
            public string? Key { get; set; }

            [Field(At = 1)]
            public string? Value { get; set; }
        }

        [Record]
        private class AnnotatedRecord
        {
            [Segment(At = 0, Key = "Key", MinOccurs = 0, MaxOccurs = -1)]
            public IDictionary<string, AnnotatedSegment>? Map { get; set; }
        }

        [Record]
        private class AnnotatedRecord2
        {
            [Segment(At = 0, Key = "Key", MinOccurs = 0, MaxOccurs = 5)]
            public IDictionary<string, AnnotatedSegment>? Map { get; set; }
        }

        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}
