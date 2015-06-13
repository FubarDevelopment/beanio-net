using System;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Bean
{
    public class BeanParserTest : ParserTest
    {
        [Fact]
        public void TestDelimitedPositions()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Bean.widget.xml");
            var reader = factory.CreateReader("w1", LoadStream("w1_position.txt"));
            try
            {
                var w = (Widget)reader.Read();
                Assert.Equal(3, w.Id);
                Assert.Equal("Widget3", w.Name);
                Assert.NotNull(w.Top);
                Assert.Equal(2, w.Top.Id);
                Assert.Equal("Widget2", w.Top.Name);
                Assert.NotNull(w.Bottom);
                Assert.Equal(1, w.Bottom.Id);
                Assert.Equal("Widget1", w.Bottom.Name);

                var text = new StringWriter();
                factory.CreateWriter("w1", text).Write(w);
                Assert.Equal(",Widget1,1,2,Widget2,Widget3,3" + Environment.NewLine, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestCollectionsAndDefaultDelmitedPositions()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Bean.widget.xml");
            var reader = factory.CreateReader("w2", LoadStream("w2_collections.txt"));
            try
            {
                var w = (Widget)reader.Read();
                Assert.Equal("3", w.Name);
                Assert.True(w.PartsList.Count > 1);
                Assert.True(w.PartsList[1].PartsList.Count > 1);
                Assert.Equal("2B", w.GetPart(1).GetPart(1).Name);

                var text = new StringWriter();
                factory.CreateWriter("w2", text).Write(w);
                Assert.Equal("1,1M,1A,1AM,1B,1BM,2,2M,2A,2AM,2B,2BM,3" + Environment.NewLine, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestFixedLengthAndOptionalFields()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Bean.widget.xml");
            var reader = factory.CreateReader("w3", LoadStream("w3_fixedLength.txt"));
            try
            {
                var w = (Widget)reader.Read();
                Assert.Equal(1, w.Id);
                Assert.Equal("name1", w.Name);
                Assert.Equal("mode1", w.Model);

                var text = new StringWriter();
                factory.CreateWriter("w3", text).Write(w);
                Assert.Equal(" 1name1mode1" + Environment.NewLine, text.ToString());

                w = (Widget)reader.Read();
                Assert.Equal(1, w.Id);
                Assert.Equal("name1", w.Name);
                Assert.Equal("mode1", w.Model);
                Assert.Collection(
                    w.PartsList,
                    part =>
                        {
                            Assert.Equal(2, part.Id);
                            Assert.Equal("name2", part.Name);
                            Assert.Equal("mode2", part.Model);
                        },
                    part =>
                        {
                            Assert.Equal(3, part.Id);
                        },
                    part =>
                        {
                            Assert.Equal(4, part.Id);
                            Assert.Equal("name4", part.Name);
                            Assert.Equal(string.Empty, part.Model);
                        });

                text = new StringWriter();
                factory.CreateWriter("w3", text).Write(w);
                Assert.Equal(" 1name1mode1 2name2mode2 3           4name4     " + Environment.NewLine, text.ToString());

                w = (Widget)reader.Read();
                text = new StringWriter();
                factory.CreateWriter("w3", text).Write(w);
                Assert.Equal(" 1name1mode1 2name2mode2 0           4name4mode4" + Environment.NewLine, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public virtual void TestFixedLengthMap()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Bean.widget.xml");
            var reader = factory.CreateReader("w4", LoadStream("w4_map.txt"));
            try
            {
                var w = (Widget)reader.Read();
                Assert.Equal(1, w.Id);
                Assert.Equal("name1", w.Name);
                Assert.NotNull(w.PartsMap);
                Assert.True(w.PartsMap.ContainsKey("part1"));
                Assert.Equal(2, w.GetPart("part1").Id);
                Assert.Equal("name2", w.GetPart("part1").Name);

                var text = new StringWriter();
                factory.CreateWriter("w4", text).Write(w);
                Assert.Equal("1name12name2" + Environment.NewLine, text.ToString());

                w = (Widget)reader.Read();
                Assert.NotNull(w.PartsMap);
                Assert.True(w.PartsMap.ContainsKey("part2"));
                Assert.Equal(3, w.GetPart("part2").Id);
                Assert.Equal("name3", w.GetPart("part2").Name);

                text = new StringWriter();
                factory.CreateWriter("w4", text).Write(w);
                Assert.Equal("1name12name23name3" + Environment.NewLine, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.Bean.{0}", fileName);
            var asm = typeof(BeanParserTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            return new StreamReader(resStream);
        }
    }
}
