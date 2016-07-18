// <copyright file="XmlGroupsTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.IO;

using Xunit;

namespace BeanIO.Parser.Xml.Groups
{
    public sealed class XmlGroupsTest : XmlParserTest
    {
        private readonly StreamFactory _factory;

        public XmlGroupsTest()
        {
            _factory = NewStreamFactory("groups_mapping.xml");
        }

        /// <summary>
        /// Test XML groups.
        /// </summary>
        [Fact]
        public void TestNestedGroups()
        {
            var reader = _factory.CreateReader("stream", LoadReader("g1_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.Equal("C", person.Type);
                writer.Write(person);

                var address = Assert.IsType<Address>(reader.Read());
                Assert.Equal("IL", address.State);
                writer.Write(address);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("David", person.FirstName);
                Assert.Equal("P", person.Type);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.Equal("P", person.Type);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("George1", person.FirstName);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("George2", person.FirstName);
                writer.Write(person);

                address = Assert.IsType<Address>(reader.Read());
                Assert.Equal("IL", address.State);
                writer.Write(address);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Kevin", person.FirstName);
                Assert.Equal("F", person.Type);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Jen", person.FirstName);
                Assert.Equal("F", person.Type);
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("g1_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test XML groups where <code>xmlType="none"</code>.
        /// </summary>
        [Fact]
        public void TestGroupXmlTypeNone()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("g2_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream2", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.Equal("C", person.Type);
                writer.Write(person);

                var address = Assert.IsType<Address>(reader.Read());
                Assert.Equal("IL", address.State);
                writer.Write(address);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("George", person.FirstName);
                Assert.Equal("F", person.Type);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Jane", person.FirstName);
                Assert.Equal("F", person.Type);
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("g2_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test an XML stream where <code>xmlType="none"</code>.
        /// </summary>
        [Fact]
        public void TestStreamXmlTypeNone()
        {
            var reader = _factory.CreateReader("stream3", LoadReader("g3_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream3", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                var address = person.Address;
                Assert.NotNull(address);
                Assert.Equal("IL", address.State);
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("g3_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
