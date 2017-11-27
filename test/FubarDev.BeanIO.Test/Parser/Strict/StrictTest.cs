// <copyright file="StrictTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

using Xunit;

namespace BeanIO.Parser.Strict
{
    public class StrictTest : ParserTest
    {
        [Fact]
        public void TestRecordLengthStrict()
        {
            var factory = NewStreamFactory("strict_mapping.xml");
            var reader = factory.CreateReader("s1_strict", LoadReader("s1_invalidRecordLength.txt"));
            try
            {
                AssertRecordError(reader, 3, "detail", "Too many fields, expected 3 maximum");
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestRecordLengthNotStrict()
        {
            var factory = NewStreamFactory("strict_mapping.xml");
            var reader = factory.CreateReader("s1_not_strict", LoadReader("s1_invalidRecordLength.txt"));
            try
            {
                reader.Read();
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestRecordSequenceStrict()
        {
            var factory = NewStreamFactory("strict_mapping.xml");
            var reader = factory.CreateReader("s1_strict", LoadReader("s1_invalidSequence.txt"));
            try
            {
                var ex = Assert.Throws<UnexpectedRecordException>(() => reader.Read());
                var ctx = ex.RecordContext;
                Assert.NotNull(ctx);
                Assert.Equal(1, ctx.LineNumber);
                Assert.Equal("detail", ctx.RecordName);
                Assert.Equal("Unexpected 'detail' record at line 1", ctx.RecordErrors.First());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestRecordSequenceNotStrict()
        {
            var factory = NewStreamFactory("strict_mapping.xml");
            var reader = factory.CreateReader("s1_not_strict", LoadReader("s1_invalidSequence.txt"));
            try
            {
                reader.Read();
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
