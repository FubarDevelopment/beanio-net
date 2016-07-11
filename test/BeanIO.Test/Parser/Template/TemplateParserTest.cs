// <copyright file="TemplateParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

using BeanIO.Builder;

using Xunit;

namespace BeanIO.Parser.Template
{
    public class TemplateParserTest : ParserTest
    {
        [Fact]
        public void TestRecordTemplate()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Template.template_mapping.xml");
            var reader = factory.CreateReader("stream1", LoadReader("t1.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(1, map["id"]);
                Assert.Equal("joe", map["name"]);
                Assert.Equal('M', map["gender"]);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestBeanTemplate()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Template.template_mapping.xml");
            var reader = factory.CreateReader("stream2", LoadReader("t1.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal('M', map["gender"]);
                Assert.NotNull(map["bean"]);
                map = Assert.IsType<Dictionary<string, object>>(map["bean"]);
                Assert.Equal(1, map["id"]);
                Assert.Equal("joe", map["name"]);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestIncludeTemplateFromRecord()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Template.template_mapping.xml");
            var reader = factory.CreateReader("stream3", LoadReader("t3.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(1, map["id"]);
                Assert.Equal("joe", map["firstName"]);
                Assert.Equal("smith", map["lastName"]);
                Assert.Equal('M', map["gender"]);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestIncludeTemplateFromRecordBuilder()
        {
            var b = new StreamBuilder("stream3", "csv")
                .AddRecord(
                    new RecordBuilder("record", typeof(Dictionary<string, object>))
                        .AddField(new FieldBuilder("id").Type(typeof(int)).At(0))
                        .LoadMapping(new Uri("resource:BeanIO.Test.Parser.Template.template_mapping.xml, BeanIO.Test"))
                        .Include("t3", 1)
                        .AddField(new FieldBuilder("gender").Type(typeof(char)).At(3)));
            var factory = StreamFactory.NewInstance();
            factory.Define(b);
            var reader = factory.CreateReader("stream3", LoadReader("t3.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal(1, map["id"]);
                Assert.Equal("joe", map["firstName"]);
                Assert.Equal("smith", map["lastName"]);
                Assert.Equal('M', map["gender"]);
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
