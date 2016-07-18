// <copyright file="DelimitedWriterTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

using BeanIO.Stream.Delimited;

using Xunit;

namespace BeanIO.Stream
{
    public class DelimitedWriterTest
    {
        private static readonly string _lineSeparator = Environment.NewLine;

        [Fact]
        public void TestDelimiterIsEscape()
        {
            var factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    Escape = ','
                };
            Assert.Throws<BeanIOConfigurationException>(() => factory.CreateWriter(new StringWriter()));
        }

        [Fact]
        public void TestDefaultConfiguration()
        {
            StringWriter text = new StringWriter();
            var writer = new DelimitedWriter(text);
            writer.Write(new[] { "value1", "value\t2" });
            Assert.Equal("value1\tvalue\t2" + _lineSeparator, text.ToString());
        }

        [Fact]
        public void TestCustomDelimiter()
        {
            StringWriter text = new StringWriter();
            var writer = new DelimitedWriter(text, ',');
            writer.Write(new[] { "value1", "value2\t", string.Empty });
            Assert.Equal("value1,value2\t," + _lineSeparator, text.ToString());
        }

        [Fact]
        public void TestCustomDelimiterAndEscape()
        {
            var factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    Escape = '\\'
                };

            StringWriter text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write(new[] { "value1", "value2," });
            Assert.Equal("value1,value2\\," + _lineSeparator, text.ToString());
        }

        [Fact]
        public void TestDefaultFactoryConfiguration()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory();
            StringWriter text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write(new[] { "value1", "value\t2" });
            Assert.Equal("value1\tvalue\t2" + _lineSeparator, text.ToString());
        }

        [Fact]
        public void TestCustomFactoryConfiguration()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    Escape = '\\',
                    RecordTerminator = string.Empty
                };
            StringWriter text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write(new[] { "value1", "value,2" });
            Assert.Equal("value1,value\\,2", text.ToString());
        }

        [Fact]
        public void TestFlushAndClose()
        {
            DelimitedRecordParserFactory factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    Escape = '\\',
                    RecordTerminator = string.Empty
                };
            StringWriter text = new StringWriter();
            var writer = factory.CreateWriter(text);
            writer.Write(new[] { "v" });
            writer.Flush();
            Assert.Equal("v", text.ToString());
            writer.Close();
        }
    }
}
