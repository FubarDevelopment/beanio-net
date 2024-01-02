// <copyright file="XmlBeansTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Xml.Bean
{
    public sealed class XmlBeansTest : XmlParserTest
    {
        private readonly StreamFactory _factory;

        public XmlBeansTest()
        {
            _factory = NewStreamFactory("beans_mapping.xml");
        }

        /// <summary>
        /// Test a nillable child bean.
        /// </summary>
        [Fact]
        public void TestNillableBean()
        {
            var reader = _factory.CreateReader("stream", LoadReader("b1_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream", s);
            try
            {
                Person person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                var address = person.Address;
                Assert.NotNull(address);
                Assert.Equal("IL", address.State);
                Assert.Equal("60610", address.Zip);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.Null(person.Address);
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("b1_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test an optional (i.e. minOccurs="0") bean with a namespace.
        /// </summary>
        [Fact]
        public void TestOptionalBeanWithNamespace()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("b2_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream2", s);
            try
            {
                Person person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                var address = person.Address;
                Assert.NotNull(address);
                Assert.Equal("IL", address.State);
                Assert.Equal("60610", address.Zip);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.Null(person.Address);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("George", person.FirstName);
                Assert.Null(person.Address);

                writer.Close();
                Assert.Equal(Load("b2_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test a nillable nested bean.
        /// </summary>
        [Fact]
        public void TestBeanCollection()
        {
            var reader = _factory.CreateReader("stream3", LoadReader("b3_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream3", s);
            try
            {
                Person person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.NotNull(person.AddressList);
                Assert.Collection(
                    person.AddressList,
                    address => Assert.Equal("IL", address.State),
                    address => Assert.Equal("CO", address.State),
                    address => Assert.Equal("MN", address.State));

                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.NotNull(person.AddressList);
                Assert.Empty(person.AddressList);
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("b3_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test a bean where xmlType="none".
        /// </summary>
        [Fact]
        public void TestXmlTypeNoneBean()
        {
            var reader = _factory.CreateReader("stream4", LoadReader("b4_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream4", s);
            try
            {
                Person person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                var address = person.Address;
                Assert.NotNull(address);
                Assert.Equal("IL", address.State);
                Assert.Equal("60610", address.Zip);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                address = person.Address;
                Assert.NotNull(address);
                Assert.Null(address.State);
                Assert.Equal(string.Empty, address.Zip);
                address.Zip = null;
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("b4_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test a nillable segment that is not bound to a bean object.
        /// </summary>
        [Fact]
        public void TestUnboundNillableSegment()
        {
            var reader = _factory.CreateReader("stream5", LoadReader("b5_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream5", s);
            try
            {
                var person = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal("John", person["firstName"]);
                Assert.Equal("IL", person["state"]);
                Assert.Equal("60610", person["zip"]);
                writer.Write(person);

                person = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal("Mary", person["firstName"]);
                Assert.False(person.ContainsKey("state"));
                Assert.False(person.ContainsKey("zip"));
                writer.Write(person);

                AssertFieldError(reader, 13, "person", "zip", null, "Expected minimum 1 occurrences");

                writer.Close();

                Assert.Equal(Load("b5_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
