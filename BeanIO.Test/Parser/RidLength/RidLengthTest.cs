using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using BeanIO.Parser.Property;

using Xunit;

namespace BeanIO.Parser.RidLength
{
    public class RidLengthTest : ParserTest
    {
        [Fact]
        public void TestRidLength()
        {
            var factory = NewStreamFactory("BeanIO.Parser.RidLength.ridlength_mapping.xml");
            var reader = factory.CreateReader("r1", LoadStream("r1.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal("acouple", reader.RecordName);
                Assert.True(map.ContainsKey("values"));
                Assert.Equal(new[] { 1, 2 }, Assert.IsType<List<int>>(map["values"]));

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal("afew", reader.RecordName);
                Assert.True(map.ContainsKey("values"));
                Assert.Equal(new[] { 1, 2, 3 }, Assert.IsType<List<int>>(map["values"]));

                Assert.IsType(typeof(Dictionary<string, object>), reader.Read());
                Assert.Equal("acouple", reader.RecordName);

                Assert.IsType(typeof(Dictionary<string, object>), reader.Read());
                Assert.Equal("afew", reader.RecordName);
            }
            finally
            {
                reader.Close();
            }
        }

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.RidLength.{0}", fileName);
            var asm = typeof(RidLengthTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            Debug.Assert(resStream != null, "resStream != null");
            return new StreamReader(resStream);
        }
    }
}
