using System.IO;

using Xunit;

namespace BeanIO.Parser.Xml.Types
{
    public sealed class XmlTypesTest : XmlParserTest
    {
        private readonly StreamFactory _factory;

        public XmlTypesTest()
        {
            _factory = NewStreamFactory("types_mapping.xml");
        }

        /// <summary>
        /// Test the various field XML types.
        /// </summary>
        [Fact]
        public void TestFieldTypesAndNillable()
        {
            var reader = _factory.CreateReader("stream", LoadReader("t1_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream", s);
            try
            {
                Person person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.Equal("Smith", person.LastName);
                Assert.Equal("M", person.Gender);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Null(person.FirstName);
                Assert.Equal("Smith", person.LastName);
                Assert.Equal("F", person.Gender);
                writer.Write(person);

                person = new Person
                    {
                        FirstName = null,
                        LastName = null,
                        Gender = "M"
                    };
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("t1_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test the attribute field types with namespaces.  Note that attribute order is not
        /// guaranteed so the output comparison may fail... (need to improve).
        /// </summary>
        [Fact]
        public void TestAttributeFieldTypes()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("t2_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream2", s);
            try
            {
                Person person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.Equal("Smith", person.LastName);
                Assert.Equal("M", person.Gender);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.Equal("Smith", person.LastName);
                Assert.Equal("F", person.Gender);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Null(person.FirstName);
                Assert.Equal(string.Empty, person.LastName);
                Assert.Null(person.Gender);
                person.LastName = null;
                writer.Write(person);

                writer.Close();

                Assert.Equal(Load("t2_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test the text field types.
        /// </summary>
        [Fact]
        public void TestTextFieldTypes()
        {
            var reader = _factory.CreateReader("stream3", LoadReader("t3_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream3", s);
            try
            {
                Person person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.Equal("M", person.Gender);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.Equal("F", person.Gender);
                writer.Write(person);

                writer.Close();

                Assert.Equal(Load("t3_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test record identification by XML attribute.
        /// </summary>
        [Fact]
        public void TestRecordIdentificationByAttribute()
        {
            var reader = _factory.CreateReader("stream4", LoadReader("t3_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream4", s);
            try
            {
                Person person = Assert.IsType<Male>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.Equal("M", person.Gender);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.Equal("F", person.Gender);
                writer.Write(person);

                writer.Close();

                Assert.Equal(Load("t3_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test record identification by XML text.
        /// </summary>
        [Fact]
        public void TestRecordIdentificationByText()
        {
            var reader = _factory.CreateReader("stream5", LoadReader("t5_in.xml"));

            try
            {
                Person person = Assert.IsType<Male>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.Equal("M", person.Gender);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.Equal("F", person.Gender);
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test record identification by XML element.
        /// </summary>
        [Fact]
        public void TestRecordIdentificationByElement()
        {
            var reader = _factory.CreateReader("stream6", LoadReader("t6_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream6", s);
            try
            {
                Person person = Assert.IsType<Male>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.Equal("M", person.Gender);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Mary", person.FirstName);
                Assert.Equal("F", person.Gender);
                writer.Write(person);

                writer.Close();

                Assert.Equal(Load("t6_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test behavior of a custom type handler where the format method
        /// may return null.
        /// </summary>
        [Fact]
        public void TestTypeHandlerNilSupport()
        {
            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream7", s);

            Person person = new Person
                {
                    FirstName = string.Empty,
                    LastName = null
                };
            writer.Write(person);

            person.FirstName = "nil";
            person.LastName = "nil";
            writer.Write(person);
            writer.Close();

            Assert.Equal(Load("t7_out.xml"), s.ToString());
        }
    }
}
