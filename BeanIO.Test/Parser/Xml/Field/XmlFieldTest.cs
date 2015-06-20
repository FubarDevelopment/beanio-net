using System.IO;

using Xunit;

namespace BeanIO.Parser.Xml.Field
{
    public sealed class XmlFieldTest : XmlParserTest
    {
        private readonly StreamFactory _factory;

        public XmlFieldTest()
        {
            _factory = NewStreamFactory("field_mapping.xml");
        }

        /// <summary>
        /// Test an optional padded field.
        /// </summary>
        [Fact]
        public void TestPaddingForOptionalField()
        {
            var reader = _factory.CreateReader("stream", LoadReader("f1_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream", s);
            try
            {
                Person person = Assert.IsType<Person>(reader.Read());
                Assert.Equal(25, person.Age);
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Null(person.Age);
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("f1_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test a required padded field.
        /// </summary>
        [Fact]
        public void TestPaddingForRequiredField()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("f2_in.xml"));

            StringWriter s = new StringWriter();
            var writer = _factory.CreateWriter("stream2", s);
            try
            {
                Person person = Assert.IsType<Person>(reader.Read());
                Assert.Equal(25, person.Age);
                writer.Write(person);

                AssertFieldError(reader, 5, "record", "age", string.Empty, "Required field not set");
                person.Age = null;
                writer.Write(person);

                AssertFieldError(reader, 8, "record", "age", "025", "Invalid padded field length, expected 5 characters");

                writer.Close();
                Assert.Equal(Load("f2_out.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Test a setter is not called if the element is missing.
        /// </summary>
        [Fact]
        public void TestSetterNotCalledForMissingField()
        {
            var reader = _factory.CreateReader("stream3", LoadReader("f3_in.xml"));

            try
            {
                Person person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("Joe", person.FirstName);
                Assert.Null(person.LastName);
                Assert.Equal(10, person.Age);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                Assert.Equal(Person.DefaultName, person.LastName);
                Assert.Equal(Person.DefaultAge, person.Age);
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
