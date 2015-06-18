using System.Collections.Generic;

using BeanIO.Config;

using NodaTime;

using Xunit;

namespace BeanIO.Parser.Substitution
{
    public class PropertySubstitutionParserTest : ParserTest
    {
        [Fact]
        public void TestRuntimeSubstitution()
        {
            var properties = new Properties(new Dictionary<string, string>()
                {
                    { "dateFormat", "yyyy-MM-dd" },
                    { "type", "string" },
                });
            var factory = StreamFactory.NewInstance();
            factory.Load(LoadStream("substitution_mapping.xml"), properties);

            var unmarshaller = factory.CreateUnmarshaller("stream");
            var map = Assert.IsType<Dictionary<string, object>>(unmarshaller.Unmarshal("2012-04-01,23,George"));
            Assert.True(map.ContainsKey("date"));
            Assert.Equal(new LocalDate(2012, 4, 1), Assert.IsType<LocalDate>(map["date"]));
            Assert.True(map.ContainsKey("age"));
            Assert.Equal("23", map["age"]);
            Assert.True(map.ContainsKey("name"));
            Assert.Equal("George", map["name"]);
        }

        [Fact]
        public void TestDefaultSubstitution()
        {
            var properties = new Properties(new Dictionary<string, string>()
                {
                    { "dateFormat", "yyyy-MM-dd" },
                });
            var factory = StreamFactory.NewInstance();
            factory.Load(LoadStream("substitution_mapping.xml"), properties);

            var unmarshaller = factory.CreateUnmarshaller("stream");
            var map = Assert.IsType<Dictionary<string, object>>(unmarshaller.Unmarshal("2012-04-01,23,George"));
            Assert.Equal(23, map["age"]);
        }

        [Fact]
        public void TestMissingProperty()
        {
            var factory = StreamFactory.NewInstance();
            Assert.Throws<BeanIOConfigurationException>(() => factory.Load(LoadStream("substitution_mapping.xml")));
        }
    }
}
