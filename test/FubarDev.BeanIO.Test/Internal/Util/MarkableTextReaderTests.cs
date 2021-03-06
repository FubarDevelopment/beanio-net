// <copyright file="MarkableTextReaderTests.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

using Xunit;

namespace BeanIO.Internal.Util
{
    public class MarkableTextReaderTests
    {
        [Fact]
        public void TestReadSimple()
        {
            var reader = new MarkableTextReader(new StringReader("a"));
            var ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));
            ch = reader.Read();
            Assert.Equal(-1, ch);
        }

        [Fact]
        public void TestPeekSimple()
        {
            var reader = new MarkableTextReader(new StringReader("a"));
            var ch = reader.Peek();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));
            ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));
            ch = reader.Read();
            Assert.Equal(-1, ch);
        }

        [Fact]
        public void TestMarkSimple()
        {
            var reader = new MarkableTextReader(new StringReader("ab"));
            reader.Mark(2);

            var ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));
            ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("b", char.ConvertFromUtf32(ch));

            reader.Reset();

            ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));
            ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("b", char.ConvertFromUtf32(ch));

            ch = reader.Read();
            Assert.Equal(-1, ch);
        }

        [Fact]
        public void TestPreliminaryReset()
        {
            var reader = new MarkableTextReader(new StringReader("ab"));
            reader.Mark(2);

            var ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));

            reader.Reset();

            ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));
            ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("b", char.ConvertFromUtf32(ch));

            ch = reader.Read();
            Assert.Equal(-1, ch);
        }

        [Fact]
        public void TestMarkExceedBufferWithRead()
        {
            var reader = new MarkableTextReader(new StringReader("ab"));
            reader.Mark(2);

            var ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));
            ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("b", char.ConvertFromUtf32(ch));
            ch = reader.Read();
            Assert.Equal(-1, ch);

            Assert.Throws<InvalidOperationException>(() => reader.Reset());
        }

        [Fact]
        public void TestMarkExceedBufferWithPeekEof()
        {
            var reader = new MarkableTextReader(new StringReader("ab"));
            reader.Mark(2);

            var ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));
            ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("b", char.ConvertFromUtf32(ch));
            ch = reader.Peek();
            Assert.Equal(-1, ch);

            reader.Reset();
        }

        [Fact]
        public void TestMarkExceedBufferWithPeek()
        {
            var reader = new MarkableTextReader(new StringReader("ab"));
            reader.Mark(1);

            var ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));
            ch = reader.Peek();
            Assert.NotEqual(-1, ch);
            Assert.Equal("b", char.ConvertFromUtf32(ch));

            reader.Reset();

            ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("a", char.ConvertFromUtf32(ch));
            ch = reader.Read();
            Assert.NotEqual(-1, ch);
            Assert.Equal("b", char.ConvertFromUtf32(ch));

            ch = reader.Read();
            Assert.Equal(-1, ch);
        }
    }
}
