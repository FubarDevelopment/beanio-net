// <copyright file="MarshalledFieldValidationTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using Xunit;

namespace BeanIO.Parser.Validation
{
    public class MarshalledFieldValidationTest : AbstractParserTest
    {
        [Fact]
        public void TestRequired()
        {
            var factory = CreateFactory(@"
            <stream name=""s"" format=""csv"" strict=""true"" validateOnMarshal=""true"">
              <record name=""record"" class=""map"">
                <field name=""field"" type=""String"" required=""true"" />
              </record>
            </stream>");
            var m = factory.CreateMarshaller("s");
            var bean = new Dictionary<string, object?>()
            {
                { "field", null },
            };
            Assert.Throws<InvalidBeanException>(() => m.Marshal(bean));
        }

        [Fact]
        public void TestMinLength()
        {
            var factory = CreateFactory(@"
            <stream name=""s"" format=""csv"" strict=""true"" validateOnMarshal=""true"">
              <record name=""record"" class=""map"">
                <field name=""field"" minLength=""3"" />
              </record>
            </stream>");
            var m = factory.CreateMarshaller("s");
            var bean = new Dictionary<string, object?>()
            {
                { "field", "ab" },
            };
            Assert.Throws<InvalidBeanException>(() => m.Marshal(bean));
        }

        [Fact]
        public void TestMaxLength()
        {
            var factory = CreateFactory(@"
            <stream name=""s"" format=""csv"" strict=""true"" validateOnMarshal=""true"">
              <record name=""record"" class=""map"">
                <field name=""field"" maxLength=""3"" />
              </record>
            </stream>");
            var m = factory.CreateMarshaller("s");
            var bean = new Dictionary<string, object?>()
            {
                { "field", "abcd" },
            };
            Assert.Throws<InvalidBeanException>(() => m.Marshal(bean));
        }

        [Fact]
        public void TestRegEx()
        {
            var factory = CreateFactory(@"
            <stream name=""s"" format=""csv"" strict=""true"" validateOnMarshal=""true"">
              <record name=""record"" class=""map"">
                <field name=""field"" regex=""\d+"" />
              </record>
            </stream>");
            var m = factory.CreateMarshaller("s");
            var bean = new Dictionary<string, object?>()
                {
                    { "field", "abc" },
                };
            Assert.Throws<InvalidBeanException>(() => m.Marshal(bean));
        }
    }
}
