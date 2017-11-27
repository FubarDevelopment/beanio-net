// <copyright file="TrimTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using Xunit;

namespace BeanIO.Parser.Trim
{
    public class TrimTest : ParserTest
    {
        [Fact]
        public void TestLazySegment()
        {
            var factory = NewStreamFactory("trim_mapping.xml");
            var u = factory.CreateUnmarshaller("s1");
            var map = Assert.IsType<Dictionary<string, object>>(u.Unmarshal("\"jen  \",jen  ,1    "));
            Assert.Equal("jen  ", map["text"]);
            Assert.Equal("jen", map["text_trim"]);
            Assert.Equal(1, map["number"]);
        }
    }
}
