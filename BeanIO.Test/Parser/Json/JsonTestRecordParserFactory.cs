using BeanIO.Stream.Json;

namespace BeanIO.Parser.Json
{
    public class JsonTestRecordParserFactory : JsonRecordParserFactory
    {
        public JsonTestRecordParserFactory()
        {
            Pretty = true;
            Indentation = 2;
            LineSeparator = "\r\n";
        }
    }
}
