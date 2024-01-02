// <copyright file="XmlBooleanTypeHandlerTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Internal.Util;
using BeanIO.Types.Xml;

using Xunit;

namespace BeanIO.Types
{
    public class XmlBooleanTypeHandlerTest
    {
        private readonly TypeHandlerFactory _factory = new TypeHandlerFactory();

        [Fact]
        public void TestParse()
        {
            var handler = _factory.GetTypeHandlerFor(typeof(bool), "xml");
            Assert.NotNull(handler);
            Assert.True(Assert.IsType<bool>(handler.Parse("true")));
            Assert.True(Assert.IsType<bool>(handler.Parse("1")));
            Assert.False(Assert.IsType<bool>(handler.Parse("false")));
            Assert.False(Assert.IsType<bool>(handler.Parse("0")));
            Assert.Null(handler.Parse(string.Empty));
            Assert.Null(handler.Parse(null));
        }

        [Fact]
        public void TestTextualFormat()
        {
            var handler = new XmlBooleanTypeHandler();
            Assert.False(handler.IsNumericFormatEnabled);
            Assert.Null(handler.Format(null));
            Assert.Equal("false", handler.Format(false));
            Assert.Equal("true", handler.Format(true));
        }

        [Fact]
        public void TestNumericFormat()
        {
            var handler = new XmlBooleanTypeHandler() { IsNumericFormatEnabled = true };
            Assert.True(handler.IsNumericFormatEnabled);
            Assert.Null(handler.Format(null));
            Assert.Equal("0", handler.Format(false));
            Assert.Equal("1", handler.Format(true));
        }
    }
}
