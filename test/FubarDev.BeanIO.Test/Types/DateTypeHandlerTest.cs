// <copyright file="DateTypeHandlerTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

using BeanIO.Config;

using Xunit;

namespace BeanIO.Types
{
    public class DateTypeHandlerTest
    {
        [Fact]
        public void TestLenient()
        {
            var handler = new DateTimeTypeHandler()
                {
                    IsLenient = true,
                    Pattern = "MM-dd-yyyy",
                };
            Assert.True(handler.IsLenient);
            Assert.Equal("MM-dd-yyyy", handler.Pattern);
        }

        [Fact]
        public void TestParsePositionPastDate()
        {
            var handler = new DateTimeTypeHandler()
                {
                    IsLenient = false,
                    Pattern = "MM-dd-yyyy",
                };
            Assert.Throws<TypeConversionException>(() => handler.Parse("01-01-2000abc"));
        }

        [Fact]
        public void TestParsePosition()
        {
            var handler = new DateTimeTypeHandler()
            {
                IsLenient = false,
                Pattern = "MM-dd-yyyy",
            };
            Assert.Throws<TypeConversionException>(() => handler.Parse("01-32-2000"));
        }

        [Fact]
        public void TestConfigure()
        {
            var handler = new DateTimeTypeHandler();
            handler.Configure(new Properties(new Dictionary<string, string?>()));
            Assert.Null(handler.Pattern);
            Assert.Null(handler.TimeZoneId);

            handler = new DateTimeTypeHandler();
            handler.Configure(
                new Properties(
                    new Dictionary<string, string?>
                        {
                            { "format", string.Empty },
                        }));
            Assert.Null(handler.Pattern);

            handler = new DateTimeTypeHandler();
            handler.Configure(
                new Properties(
                    new Dictionary<string, string?>
                        {
                            { "format", "yyyy-MM-dd" },
                        }));
            Assert.Equal("yyyy-MM-dd", handler.Pattern);
        }

        [Fact]
        public void TestInvalidPattern()
        {
            var handler = new DateTimeTypeHandler();
            Assert.Throws<ArgumentException>(() => handler.Pattern = "sss");
        }
    }
}
