// <copyright file="NumberTypeHandlerTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Globalization;

using Xunit;

namespace BeanIO.Types
{
    public class NumberTypeHandlerTest
    {
        [Fact]
        public void TestParseInvalid()
        {
            var handler = new IntegerTypeHandler();
            Assert.Throws<TypeConversionException>(() => handler.Parse("abc"));
        }

        [Fact]
        public void TestParseHexPattern()
        {
            var handler = new IntegerTypeHandler
                {
                    Pattern = Tuple.Create(NumberStyles.HexNumber, "X")
                };
            Assert.Equal(16, handler.Parse("10"));
        }

        [Fact]
        public void TestParseInvalidIncomplete()
        {
            var handler = new IntegerTypeHandler
            {
                Pattern = Tuple.Create(NumberStyles.Any, "0")
            };
            Assert.Throws<TypeConversionException>(() => handler.Parse("10a"));
        }
    }
}
