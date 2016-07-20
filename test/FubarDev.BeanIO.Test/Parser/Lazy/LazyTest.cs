// <copyright file="LazyTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using Xunit;

namespace BeanIO.Parser.Lazy
{
    public class LazyTest : ParserTest
    {
        [Fact]
        public void TestLazySegment()
        {
            var factory = NewStreamFactory("lazy_mapping.xml");
            var u = factory.CreateUnmarshaller("s1");

            var user = Assert.IsType<LazyUser>(u.Unmarshal("kevin          "));
            Assert.Equal("kevin", user.name);
            Assert.Null(user.account);

            user = Assert.IsType<LazyUser>(u.Unmarshal("kevin1         "));
            Assert.Equal("kevin", user.name);
            Assert.NotNull(user.account);
            Assert.Equal(1, user.account.Number);
            Assert.Equal(string.Empty, user.account.Text);
        }

        [Fact]
        public void TestRepeatingLazySegments()
        {
            var factory = NewStreamFactory("lazy_mapping.xml");
            var u = factory.CreateUnmarshaller("s2");

            var user = Assert.IsType<LazyUser>(u.Unmarshal("kevin      "));
            Assert.Equal("kevin", user.name);
            Assert.Null(user.account);

            user = Assert.IsType<LazyUser>(u.Unmarshal("kevin   001"));
            Assert.Equal("kevin", user.name);
            Assert.NotNull(user.accounts);
            Assert.Collection(
                user.accounts,
                item => Assert.Equal(1, item.Number));
        }

        [Fact]
        public void TestNestedLazySegments()
        {
            var factory = NewStreamFactory("lazy_mapping.xml");
            var u = factory.CreateUnmarshaller("s3");

            var user = Assert.IsType<LazyUser>(u.Unmarshal("kevin,7,checking,DR,CR"));
            Assert.Equal("kevin", user.name);
            Assert.NotNull(user.account);
            Assert.Equal(7, user.account.Number);
            Assert.Equal("checking", user.account.Text);
            Assert.NotNull(user.account.Transactions);
            Assert.Collection(
                user.account.Transactions,
                item => Assert.Equal("DR", item.Type),
                item => Assert.Equal("CR", item.Type));

            user = Assert.IsType<LazyUser>(u.Unmarshal("kevin,,,,"));
            Assert.Equal("kevin", user.name);
            Assert.Null(user.account);
        }

        [Fact]
        public void TestRepeatingLazyField()
        {
            var factory = NewStreamFactory("lazy_mapping.xml");
            var u = factory.CreateUnmarshaller("s4");
            var record = Assert.IsType<Dictionary<string, object>>(u.Unmarshal("kevin,trevor"));
            Assert.True(record.ContainsKey("names"));
            var names = Assert.IsType<List<string>>(record["names"]);
            Assert.Equal(new[] { "kevin", "trevor" }, names);

            record = Assert.IsType<Dictionary<string, object>>(u.Unmarshal(","));
            Assert.Equal(0, record.Count);
            Assert.False(record.ContainsKey("names"));
        }
    }
}
