using System.Diagnostics;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Delimited
{
    public class DelimitedParserTest : ParserTest
    {
        [Fact]
        public void TestRequiredField()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Delimited.delimited.xml");
            var reader = factory.CreateReader("d1", LoadStream("d1_recordErrors.txt"));
            try
            {
                AssertRecordError(reader, 1, "record1", "Too few fields 2");
                AssertRecordError(reader, 2, "record1", "Too many fields 4");
                AssertFieldError(reader, 3, "record1", "field4", null, "Expected minimum 1 ocurrences");
                AssertFieldError(reader, 4, "record1", "field4", string.Empty, "Required field not set");
            }
            finally
            {
                reader.Close();
            }
        }

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.Delimited.{0}", fileName);
            var asm = typeof(DelimitedParserTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            Debug.Assert(resStream != null, "resStream != null");
            return new StreamReader(resStream);
        }
    }
}
