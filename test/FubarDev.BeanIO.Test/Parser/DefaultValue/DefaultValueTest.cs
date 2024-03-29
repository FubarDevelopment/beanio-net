// <copyright file="DefaultValueTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using System.Reflection;

using Xunit;

namespace BeanIO.Parser.DefaultValue
{
    public class DefaultValueTest : AbstractParserTest
    {
        [Fact]
        public void TestFieldWithDefault()
        {
            var factory = CreateFactory(@"
            <stream name=""s"" format=""csv"" strict=""true"">
              <record name=""record"" class=""BeanIO.Beans.Bean, FubarDev.BeanIO.Test"">
                <field name=""field1"" default=""default1"" minOccurs=""1"" />
                <field name=""field2"" default=""default2"" minOccurs=""0"" />
                <field name=""date"" default=""00000000"" minOccurs=""0"" format=""yyyyMMdd"" parseDefault=""false"" />
              </record>
            </stream>");

            var u = factory.CreateUnmarshaller("s");
            var m = factory.CreateMarshaller("s");

            var bean = (Beans.Bean?)u.Unmarshal("value1,value2,00000000");
            Assert.NotNull(bean);
            Assert.Equal("value1", bean.field1);
            Assert.Equal("value2", bean.GetType().GetField("field2", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(bean));
            Assert.Equal("value1,value2,00000000", m.Marshal(bean).ToString());

            bean = (Beans.Bean?)u.Unmarshal(string.Empty);
            Assert.NotNull(bean);
            Assert.Equal("default1", bean.field1);
            Assert.Equal("default2", bean.GetType().GetField("field2", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(bean));
            Assert.Equal("default1,default2,00000000", m.Marshal(bean).ToString());
        }

        [Fact]
        public void TestRepeatingFieldWithDefault()
        {
            var factory = CreateFactory(@"
            <stream name=""s"" format=""csv"" strict=""true"">
              <record name=""record"" class=""BeanIO.Beans.Bean, FubarDev.BeanIO.Test"">
                <field name=""list"" collection=""list"" default=""default"" minOccurs=""1"" maxOccurs=""5"" />
              </record>
            </stream>");

            var u = factory.CreateUnmarshaller("s");
            var m = factory.CreateMarshaller("s");

            var bean = (Beans.Bean?)u.Unmarshal("value1,value2");
            Assert.NotNull(bean);
            Assert.NotNull(bean.list);
            Assert.Equal(new[] { "value1", "value2" }, bean.list.Cast<string>());
            Assert.Equal("value1,value2", m.Marshal(bean).ToString());

            bean = (Beans.Bean?)u.Unmarshal(string.Empty);
            Assert.NotNull(bean);
            Assert.NotNull(bean.list);
            Assert.Equal(new[] { "default" }, bean.list.Cast<string>());
            Assert.Equal("default", m.Marshal(bean).ToString());
        }
    }
}
