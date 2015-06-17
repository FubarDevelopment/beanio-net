using System;
using System.Collections.Generic;

using Xunit;

namespace BeanIO.Parser.Types
{
    public class TypeHandlerLocaleTest : AbstractParserTest
    {
        [Fact]
        public void TestFieldWithDefault()
        {
            var date = new DateTime(2013, 2, 1);

            var factory = CreateFactory(@"
                <stream name=""s"" format=""csv"" strict=""true"">
                  <typeHandler name=""int_de"" class=""BeanIO.Types.IntegerTypeHandler, BeanIO"">
                    <property name=""locale"" value=""de"" />
                  </typeHandler>
                  <typeHandler name=""date_de"" class=""BeanIO.Types.DateTimeTypeHandler, BeanIO"">
                    <property name=""locale"" value=""de"" />
                  </typeHandler>
                  <record name=""record"" class=""map"">
                    <field name=""int1"" typeHandler=""int_de"" format=""#,##0"" />
                    <field name=""int2"" type=""int"" format=""#,##0"" />
                    <field name=""date"" typeHandler=""date_de"" />
                  </record>
                </stream>");

            var text = "10.000,\"10,000\",01.02.2013 00:00:00";
            var map = new Dictionary<string, object>()
                {
                    { "int1", 10000 },
                    { "int2", 10000 },
                    { "date", date },
                };

            var m = factory.CreateMarshaller("s");
            Assert.Equal(text, m.Marshal(map).ToString());

            var u = factory.CreateUnmarshaller("s");
            Assert.Equal(map, u.Unmarshal(text));
        }
    }
}
