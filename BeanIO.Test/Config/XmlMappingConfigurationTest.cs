using BeanIO.Parser;

using Xunit;

namespace BeanIO.Config
{
    public class XmlMappingConfigurationTest : ParserTest
    {
        [Fact]
        public void TestTemplateImport()
        {
            var factory = StreamFactory.NewInstance();
            using (var stream = typeof(ParserTest).Assembly.GetManifestResourceStream("BeanIO.Config.ab.xml"))
            {
                factory.Load(stream);
            }
        }

        [Fact]
        public void TestImport()
        {
            var factory = StreamFactory.NewInstance();
            using (var stream = typeof(ParserTest).Assembly.GetManifestResourceStream("BeanIO.Config.import.xml"))
            {
                factory.Load(stream);
            }
        }
    }
}
