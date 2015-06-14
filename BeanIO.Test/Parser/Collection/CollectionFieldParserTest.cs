using System;
using System.IO;

using BeanIO.Parser.Bean;

using Xunit;

namespace BeanIO.Parser.Collection
{
    public class CollectionFieldParserTest : ParserTest
    {
        [Fact]
        public void TestCollectionDelimited()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Collection.collection.xml");
            var reader = factory.CreateReader("dc1", LoadStream("dc1_valid.txt"));
            try
            {
                var bean = (CollectionBean)reader.Read();
                Assert.Equal(new[] { "George", "Gary", "Jon" }, bean.List);
                Assert.Equal(new[] { 1, 2, 3, 4 }, bean.Array);

                var text = new StringWriter();
                factory.CreateWriter("dc1", text).Write(bean);
                Assert.Equal("George,Gary,Jon,1,2,3,4" + Environment.NewLine, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestNullPrimitive()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Collection.collection.xml");
            var reader = factory.CreateReader("dc2", LoadStream("dc2_nullPrimitive.txt"));
            try
            {
                var bean = (CollectionBean)reader.Read();
                Assert.Equal(new[] { 1, 0, 3 }, bean.Array);

                var text = new StringWriter();
                factory.CreateWriter("dc2", text).Write(new CollectionBean());
                Assert.Equal(",," + Environment.NewLine, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.Collection.{0}", fileName);
            var asm = typeof(BeanParserTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            return new StreamReader(resStream);
        }
    }
}
