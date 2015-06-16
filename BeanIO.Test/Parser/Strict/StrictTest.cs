﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using Xunit;

namespace BeanIO.Parser.Strict
{
    public class StrictTest : ParserTest
    {
        [Fact]
        public void TestRecordLengthStrict()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Strict.strict_mapping.xml");
            var reader = factory.CreateReader("s1_strict", LoadStream("s1_invalidRecordLength.txt"));
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
            var factory = NewStreamFactory("BeanIO.Parser.Strict.strict_mapping.xml");
            var reader = factory.CreateReader("s1_not_strict", LoadStream("s1_invalidRecordLength.txt"));
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
            var factory = NewStreamFactory("BeanIO.Parser.Strict.strict_mapping.xml");
            var reader = factory.CreateReader("s1_strict", LoadStream("s1_invalidSequence.txt"));
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
            var factory = NewStreamFactory("BeanIO.Parser.Strict.strict_mapping.xml");
            var reader = factory.CreateReader("s1_not_strict", LoadStream("s1_invalidSequence.txt"));
            try
            {
                reader.Read();
            }
            finally
            {
                reader.Close();
            }
        }

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.Strict.{0}", fileName);
            var asm = typeof(StrictTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            Debug.Assert(resStream != null, "resStream != null");
            return new StreamReader(resStream);
        }
    }
}