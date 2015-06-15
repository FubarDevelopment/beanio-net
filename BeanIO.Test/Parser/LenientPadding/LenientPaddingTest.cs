using System.Reflection;

using Xunit;

namespace BeanIO.Parser.LenientPadding
{
    public class LenientPaddingTest : AbstractParserTest
    {
        [Fact]
        public void TestLazySegmentMap()
        {
            var factory = CreateFactory(@"
                <stream name=""s"" format=""fixedlength"" strict=""true"">
                  <record name=""record"" class=""BeanIO.Beans.Bean, BeanIO.Test"">
                    <field name=""field1"" length=""3"" />
                    <field name=""field2"" length=""3"" minOccurs=""0"" lenientPadding=""true"" />
                    <field name=""field3"" length=""3"" minOccurs=""0"" lenientPadding=""true"" />
                  </record>
                </stream>");

            var u = factory.CreateUnmarshaller("s");
            var obj = Assert.IsType<Beans.Bean>(u.Unmarshal("aaabb"));
            Assert.Equal("aaa", obj.field1);
            Assert.Equal("bb", typeof(Beans.Bean).GetField("field2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj));

            obj = Assert.IsType<Beans.Bean>(u.Unmarshal("aaabb c"));
            Assert.Equal("aaa", obj.field1);
            Assert.Equal("bb", typeof(Beans.Bean).GetField("field2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj));
            Assert.Equal("c", obj.field3);

            obj = Assert.IsType<Beans.Bean>(u.Unmarshal("aaa"));
            Assert.Equal("aaa", obj.field1);
            Assert.Equal(null, typeof(Beans.Bean).GetField("field2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj));
            Assert.Equal(null, obj.field3);

            Assert.Throws<InvalidRecordException>(() => u.Unmarshal("aa"));
        }
    }
}
