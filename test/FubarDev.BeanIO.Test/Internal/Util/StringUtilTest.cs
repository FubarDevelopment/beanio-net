// <copyright file="StringUtilTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

using BeanIO.Config;

using Xunit;

namespace BeanIO.Internal.Util
{
    public class StringUtilTest
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" no prop ", " no prop ")]
        [InlineData("${1}", "1")]
        [InlineData(" ${1} ${2} ", " 1 2 ")]
        [InlineData("${1} ${2", "1 ${2")]
        [InlineData("${}", "empty")]
        [InlineData("${ space }", " ")]
        [InlineData("-$}", "-$}")]
        [InlineData("${missing,int}", "int")]
        [InlineData("${missing, }", " ")]
        [InlineData("${missing,1}", "1")]
        [InlineData("${missing,}", "")]
        [InlineData("$", "$")]
        public void TestSuccessful(string? source, string? expected)
        {
            var props = new Properties(new Dictionary<string, string?>()
            {
                { "1", "1" },
                { "2", "2" },
                { string.Empty, "empty" },
                { " space ", " " },
            });

            Assert.Equal(expected, StringUtil.DoPropertySubstitution(source, props));
        }

        [Fact]
        public void TestMissingProperty()
        {
            Assert.Throws<ArgumentException>(() => StringUtil.DoPropertySubstitution("${missing}", (Properties?)null));
        }
    }
}
