// <copyright file="TypeHandlerLocaleTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
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
                  <typeHandler name=""int_de"" class=""BeanIO.Types.IntegerTypeHandler, FubarDev.BeanIO"">
                    <property name=""locale"" value=""de"" />
                  </typeHandler>
                  <typeHandler name=""date_de"" class=""BeanIO.Types.DateTimeTypeHandler, FubarDev.BeanIO"">
                    <property name=""locale"" value=""de"" />
                  </typeHandler>
                  <record name=""record"" class=""map"">
                    <field name=""int1"" typeHandler=""int_de"" format=""#,##0"" />
                    <field name=""int2"" type=""int"" format=""#,##0"" />
                    <field name=""date"" typeHandler=""date_de"" />
                  </record>
                </stream>");

            var cultureDe = new CultureInfo("de");
            var text = string.Format("10.000,\"10,000\",{0}", date.ToString(cultureDe));
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
