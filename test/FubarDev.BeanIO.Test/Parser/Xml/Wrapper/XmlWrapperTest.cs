// <copyright file="XmlWrapperTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Xml.Wrapper
{
    /// <summary>
    /// Test cases for testing XML wrapper elements
    /// </summary>
    public sealed class XmlWrapperTest : XmlParserTest
    {
        private readonly IStreamFactory _factory;

        public XmlWrapperTest()
        {
            _factory = NewStreamFactory("wrapper_mapping.xml");
        }

        /// <summary>
        /// Test a xmlWrapper configuration for various field types.
        /// </summary>
        [Fact]
        public void TestFieldCollectionNotNillable()
        {
            var reader = _factory.CreateReader("stream", LoadReader("w1_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.Equal("Smith", person.LastName);
                var list = person.Color;
                Assert.Equal(new[] { "Red", "Blue", "Green" }, list);
                var addressList = person.AddressList;
                Assert.Null(addressList);
                ////Assert.Equal(0, addressList.size());
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal(string.Empty, person.FirstName);
                Assert.Equal(Person.DefaultName, person.LastName);
                Assert.Empty(person.Color);
                addressList = person.AddressList;
                Assert.Collection(
                    addressList,
                    item => Assert.Equal("CO", item.State),
                    item => Assert.Equal("IL", item.State));
                person.LastName = null;
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("w1_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test a xmlWrapper configuration for a nillable field collection.
        /// </summary>
        [Fact]
        public void TestFieldCollectionNillable()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("w2_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream2", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                var list = person.Color;
                Assert.Equal(new[] { "Red", "Blue" }, list);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Null(person.Color);
                ////assertNotNull(person.Color);
                ////Assert.Equal(0, person.Color.size());
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("w2_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test a xmlWrapper configuration for a field collection where min occurs is one.
        /// </summary>
        [Fact]
        public void TestFieldCollectionMinOccursOne()
        {
            var reader = _factory.CreateReader("stream3", LoadReader("w3_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream3", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                var list = person.Color;
                Assert.Equal(new[] { string.Empty }, list);
                writer.Write(person);

                person.Color = new List<string>();
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("w3_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
