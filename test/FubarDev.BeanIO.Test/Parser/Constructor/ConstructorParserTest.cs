// <copyright file="ConstructorParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Builder;

using Xunit;

namespace BeanIO.Parser.Constructor
{
    public class ConstructorParserTest : AbstractParserTest
    {
        [Fact]
        public void TestConstructor()
        {
            var factory = CreateFactory(@"
          <stream name=""c1"" format=""csv"">
            <record name=""record"" class=""BeanIO.Parser.Constructor.Color, BeanIO.Test"">
              <field name=""name"" setter=""#1"" />
              <field name=""r"" type=""int"" setter=""#2"" />
              <field name=""g"" type=""int"" setter=""#3"" />
              <field name=""b"" type=""int"" setter=""#4"" />
            </record>
          </stream>");

            var u = factory.CreateUnmarshaller("c1");
            var color = (Color)u.Unmarshal("red,255,0,0");
            Assert.Equal("red", color.Name);
            Assert.Equal(255, color.R);
            Assert.Equal(0, color.G);
            Assert.Equal(0, color.B);
        }

        [Fact]
        public void TestAnnotatedConstructor()
        {
            var factory = CreateFactory(
                new StreamBuilder("c1")
                    .Format("csv")
                    .AddRecord(typeof(AnnotatedColor)));

            var u = factory.CreateUnmarshaller("c1");
            var color = (AnnotatedColor)u.Unmarshal("red,255,0,0");
            Assert.Equal("red", color.Name);
            Assert.Equal(255, color.R);
            Assert.Equal(0, color.G);
            Assert.Equal(0, color.B);
        }
    }
}
