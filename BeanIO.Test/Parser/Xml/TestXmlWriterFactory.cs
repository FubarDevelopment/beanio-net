using BeanIO.Stream.Xml;

namespace BeanIO.Parser.Xml
{
    public class TestXmlWriterFactory : XmlRecordParserFactory
    {
        public TestXmlWriterFactory()
        {
            SuppressHeader = true;
            LineSeparator = "\r\n";
            Indentation = 2;
        }
    }
}
