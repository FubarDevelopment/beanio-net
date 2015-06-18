using System;
using System.IO;

using NodaTime;

using Xunit;

namespace BeanIO.Parser.WriteMode
{
    public class WriteModeParserTest : ParserTest
    {
        [Fact]
        public void TestBasic()
        {
            var factory = NewStreamFactory("BeanIO.Parser.WriteMode.writemode_mapping.xml");
            var person = new TestPerson();
            var text = new StringWriter();
            factory.CreateWriter("wm1", text).Write(person);
            Assert.Equal("John,Smith,21,2011-01-01" + LineSeparator, text.ToString());
        }

        [Fact]
        public void TestCreateReader()
        {
            var factory = NewStreamFactory("BeanIO.Parser.WriteMode.writemode_mapping.xml");
            Assert.Throws<BeanIOException>(() => factory.CreateReader("wm1", new StringReader("dummy")));
        }

        private class TestPerson : IPerson
        {
            private readonly LocalDate _birthDate = new LocalDate(2011, 1, 1);

            public string FirstName
            {
                get { return "John"; }
            }

            public string LastName
            {
                get { return "Smith"; }
            }

            public int Age
            {
                get { return 21; }
            }

            public LocalDate BirthDate
            {
                get { return _birthDate; }
            }
        }
    }
}
