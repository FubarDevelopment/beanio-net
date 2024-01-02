// <copyright file="DirectParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using Xunit;

namespace BeanIO.Parser.Direct
{
    public class DirectParserTest : ParserTest
    {
        [Fact]
        public void TestPadding()
        {
            var factory = NewStreamFactory("direct_mapping.xml");
            var u = factory.CreateUnmarshaller("d1");
            var p = (DirectUser?)u.Unmarshal("george,true");
            Assert.NotNull(p);
            Assert.Equal("george", p.FirstName);
            Assert.True(p.Enabled);
        }
    }
}
