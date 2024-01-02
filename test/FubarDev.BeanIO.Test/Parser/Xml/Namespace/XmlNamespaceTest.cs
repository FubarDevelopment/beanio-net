// <copyright file="XmlNamespaceTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.IO;

using Xunit;

namespace BeanIO.Parser.Xml.Namespace
{
    public sealed class XmlNamespaceTest : XmlParserTest
    {
        private readonly StreamFactory _factory;

        public XmlNamespaceTest()
        {
            _factory = NewStreamFactory("namespace_mapping.xml");
        }

        /// <summary>
        /// Test no root namespace declaration.
        /// </summary>
        [Fact]
        public void TestNoRootNamespace()
        {
            var reader = _factory.CreateReader("stream", LoadReader("ns1_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);

                writer.Write(person);
                writer.Close();

                Assert.Equal(Load("ns1_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test root namespace declaration of '*'.
        /// </summary>
        [Fact]
        public void TestAnyRootNamespace()
        {
            var reader = _factory.CreateReader("stream1", LoadReader("ns1_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream1", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);

                writer.Write(person);
                writer.Close();

                Assert.Equal(Load("ns1_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test namespace declarations at all levels.
        /// </summary>
        [Fact]
        public void TestExactNamespace()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("ns1_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream2", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);

                writer.Write(person);
                writer.Close();

                Assert.Equal(Load("ns2_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test any namespace.
        /// </summary>
        [Fact]
        public void TestRecordNamespaceAny()
        {
            var reader = _factory.CreateReader("stream3", LoadReader("ns1_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream3", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);

                writer.Write(person);
                writer.Close();

                Assert.Equal(Load("ns3_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test a field namespace of '*'.
        /// </summary>
        [Fact]
        public void TestFieldNamespaceAny()
        {
            var reader = _factory.CreateReader("stream4", LoadReader("ns1_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream4", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);

                writer.Write(person);
                writer.Close();

                Assert.Equal(Load("ns4_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test a group namespace of '*'.
        /// </summary>
        [Fact]
        public void TestGroupNamespaceAny()
        {
            var reader = _factory.CreateReader("stream5", LoadReader("ns1_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream5", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);

                writer.Write(person);
                writer.Close();

                Assert.Equal(Load("ns5_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestStreamNamespaceDoesNotMatch()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("ns_noMatchingStream.xml"));
            try
            {
                Assert.Throws<UnidentifiedRecordException>(() => reader.Read());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestGroupNamespaceDoesNotMatch()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("ns_noMatchingGroup.xml"));

            try
            {
                Assert.Throws<UnidentifiedRecordException>(() => reader.Read());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestRecordNamespaceDoesNotMatch()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("ns_noMatchingRecord.xml"));

            try
            {
                Assert.Throws<UnidentifiedRecordException>(() => reader.Read());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestFieldNamespaceDoesNotMatch()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("ns_noMatchingField.xml"));

            try
            {
                // since the field is not used to identify the record, a bean is
                // still created and the field (with the non-matching namespace)
                // is never populated on the bean
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Null(person.FirstName);
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test a namespace prefix set at the record level.
        /// </summary>
        [Fact]
        public void TestNamespacePrefixRecord()
        {
            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream6", s);

            var person = new Person { FirstName = "John" };
            writer.Write(person);

            person.FirstName = "David";
            writer.Write(person);
            writer.Close();

            Assert.Equal(Load("ns6_out.xml"), s.ToString());
        }

        /// <summary>
        /// Test a namespace prefix set at the field level.
        /// </summary>
        [Fact]
        public void TestNamespacePrefixField()
        {
            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream7", s);

            var person = new Person { FirstName = "John" };
            writer.Write(person);
            writer.Close();

            Assert.Equal(Load("ns7_out.xml"), s.ToString());
        }

        /// <summary>
        /// Test a namespace prefix set at the stream level.  Also test XML header
        /// with overridden values.
        /// </summary>
        [Fact]
        public void TestNamespacePrefixStream()
        {
            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream8", s);

            var person = new Person { FirstName = "John" };
            writer.Write(person);
            writer.Close();

            Assert.Equal(Load("ns8_out.xml"), s.ToString());
        }

        /// <summary>
        /// Test a namespace prefix set at the group level.  Also test XML header without
        /// encoding.
        /// </summary>
        [Fact]
        public void TestNamespacePrefixGroup()
        {
            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream9", s);

            var person = new Person { FirstName = "John" };
            writer.Write(person);
            writer.Close();

            Assert.Equal(Load("ns9_out.xml"), s.ToString());
        }

        /// <summary>
        /// Test namespace declarations on the root element.  Also test default XML
        /// header values.
        /// </summary>
        [Fact]
        public void TestEagerNamespaceDeclaration()
        {
            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream10", s);

            var person = new Person { LastName = "Smith" };
            writer.Write(person);
            writer.Close();

            AssertXmlEquals(Load("ns10_out.xml"), s.ToString());
        }

        /// <summary>
        /// Test xmlPrefix="".
        /// </summary>
        [Fact]
        public void TestEmptyPrefix()
        {
            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream11", s);

            var person = new Person
                {
                    FirstName = "Joe",
                    LastName = "Smith"
                };
            writer.Write(person);
            writer.Close();

            AssertXmlEquals(Load("ns11_out.xml"), s.ToString());
        }

        /// <summary>
        /// Test xmlPrefix="".
        /// </summary>
        [Fact]
        public void TestEmptyPrefix2()
        {
            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream12", s);

            Person person = new Person()
                {
                    Address = new Address { City = "San Francisco" },
                };
            writer.Write(person);
            writer.Close();

            AssertXmlEquals(Load("ns12_out.xml"), s.ToString());
        }

        /// <summary>
        /// Test xmlPrefix="".
        /// </summary>
        [Fact]
        public void TestEmptyPrefix3()
        {
            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream13", s);

            Person person = new Person()
                {
                    Address = new Address { City = "San Francisco" },
                };
            writer.Write(person);
            writer.Close();

            AssertXmlEquals(Load("ns13_out.xml"), s.ToString());
        }
    }
}
