// <copyright file="DelimitedRecordParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Stream.Delimited;

using Xunit;

namespace BeanIO.Stream
{
    public class DelimitedRecordParserTest
    {
        private readonly DelimitedRecordParserFactory _factory = new DelimitedRecordParserFactory();

        [Fact]
        public void TestUnmarshalBasic()
        {
            var unmarshaller = _factory.CreateUnmarshaller();
            var actual = Assert.IsType<string[]>(unmarshaller.Unmarshal("1\t2\t33\t444\t"));

            var expected = new[] { "1", "2", "33", "444", string.Empty };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestUnmarshalEscapeDisabled()
        {
            var factory = new DelimitedRecordParserFactory { Escape = null };

            var unmarshaller = factory.CreateUnmarshaller();
            var actual = Assert.IsType<string[]>(unmarshaller.Unmarshal("1\\\\\t2"));

            Assert.Equal(new[] { "1\\\\", "2" }, actual);
        }

        [Fact]
        public void TestUnmarshalEscapeEscape()
        {
            var factory = new DelimitedRecordParserFactory { Escape = '\\' };

            var unmarshaller = factory.CreateUnmarshaller();
            var actual = Assert.IsType<string[]>(unmarshaller.Unmarshal("1\\\\\t2"));

            Assert.Equal(new[] { "1\\", "2" }, actual);
        }

        [Fact]
        public void TestUnmarshalEscapeDelimiter()
        {
            var factory = new DelimitedRecordParserFactory { Escape = '\\' };

            var unmarshaller = factory.CreateUnmarshaller();
            var actual = Assert.IsType<string[]>(unmarshaller.Unmarshal("1\\\t\t2\\"));

            Assert.Equal(new[] { "1\t", "2\\" }, actual);
        }

        [Fact]
        public void TestUnmarshalEscapeOther()
        {
            var factory = new DelimitedRecordParserFactory { Escape = '\\' };

            var unmarshaller = factory.CreateUnmarshaller();
            var actual = Assert.IsType<string[]>(unmarshaller.Unmarshal("1\t2\\2"));

            Assert.Equal(new[] { "1", "2\\2" }, actual);
        }

        [Fact]
        public void TestUnmarshalCustomDelimiter()
        {
            var factory = new DelimitedRecordParserFactory { Delimiter = ',' };

            var unmarshaller = factory.CreateUnmarshaller();
            var actual = Assert.IsType<string[]>(unmarshaller.Unmarshal("1,2,\t3"));

            Assert.Equal(new[] { "1", "2", "\t3" }, actual);
        }

        [Fact]
        public void TestUnmarshalDelimiterIsEscape()
        {
            var factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    Escape = ','
                };
            Assert.Throws<BeanIOConfigurationException>(() => factory.CreateUnmarshaller());
        }

        [Fact]
        public void TestMarshalDelimiterIsEscape()
        {
            var factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    Escape = ','
                };
            Assert.Throws<BeanIOConfigurationException>(() => factory.CreateMarshaller());
        }

        [Fact]
        public void TestMarshalDefaultConfiguration()
        {
            var marshaller = _factory.CreateMarshaller();
            string record = marshaller.Marshal(new[] { "value1", "value\t2" });
            Assert.Equal("value1\tvalue\t2", record);
        }

        [Fact]
        public void TestMarshalCustomDelimiter()
        {
            var factory = new DelimitedRecordParserFactory { Delimiter = ',' };

            var marshaller = factory.CreateMarshaller();
            string record = marshaller.Marshal(new[] { "value1", "value2\t", string.Empty });
            Assert.Equal("value1,value2\t,", record);
        }

        [Fact]
        public void TestMarshalCustomDelimiterAndEscape()
        {
            var factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    Escape = '\\'
                };

            var marshaller = factory.CreateMarshaller();
            string record = marshaller.Marshal(new[] { "value1", "value2," });
            Assert.Equal("value1,value2\\,", record);
        }

        [Fact]
        public void TestMarshalDefaultFactoryConfiguration()
        {
            var marshaller = _factory.CreateMarshaller();
            string record = marshaller.Marshal(new[] { "value1", "value\t2" });
            Assert.Equal("value1\tvalue\t2", record);
        }

        [Fact]
        public void TestMarshalCustomFactoryConfiguration()
        {
            var factory = new DelimitedRecordParserFactory
                {
                    Delimiter = ',',
                    Escape = '\\',
                    RecordTerminator = string.Empty
                };

            var marshaller = factory.CreateMarshaller();
            string record = marshaller.Marshal(new[] { "value1", "value,2" });
            Assert.Equal("value1,value\\,2", record);
        }
    }
}
