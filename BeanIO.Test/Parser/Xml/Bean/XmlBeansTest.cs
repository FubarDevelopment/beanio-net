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
            _factory = NewStreamFactory("BeanIO.Parser.Xml.Bean.beans_mapping.xml");
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
                Address address = person.Address;
                Assert.Equal("IL", address.State);
                Assert.Equal("60610", address.Zip);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.Null(person.Address);
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("BeanIO.Parser.Xml.Bean.b1_out.xml"), s.ToString());
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
                Address address = person.Address;
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
                Assert.Equal(Load("BeanIO.Parser.Xml.Bean.b2_out.xml"), s.ToString());
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
                Assert.Equal(3, person.AddressList.Count);
                int i = 0;
                foreach (var address in person.AddressList)
                {
                    switch (++i)
                    {
                        case 1:
                            Assert.Equal("IL", address.State);
                            break;
                        case 2:
                            Assert.Equal("CO", address.State);
                            break;
                        case 3:
                            Assert.Equal("MN", address.State);
                            break;
                    }
                }
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.Equal(0, person.AddressList.Count);
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("BeanIO.Parser.Xml.Bean.b3_in.xml"), s.ToString());
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
                Address address = person.Address;
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
                Assert.Equal(Load("BeanIO.Parser.Xml.Bean.b4_in.xml"), s.ToString());
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

                Assert.Equal(Load("BeanIO.Parser.Xml.Bean.b5_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
