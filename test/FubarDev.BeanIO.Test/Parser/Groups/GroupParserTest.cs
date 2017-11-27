// <copyright file="GroupParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using Xunit;

namespace BeanIO.Parser.Groups
{
    public class GroupParserTest : ParserTest
    {
        [Fact]
        public void TestEmptyFile()
        {
            Test("g1", "g1_empty.txt");
        }

        [Fact]
        public void TestMissingOptionalGroup()
        {
            Test("g1", "g1_nobatch.txt");
        }

        [Fact]
        public void TestMissingTrailer()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g1", "g1_noTrailer.txt", 2));
        }

        [Fact]
        public void TestMissingHeader()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g1", "g1_noHeader.txt", 1));
        }

        [Fact]
        public void TestTooManyRecords()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g1", "g1_tooManyHeaders.txt", 2));
        }

        [Fact]
        public void TestOneBatch()
        {
            Test("g1", "g1_oneBatch.txt");
        }

        [Fact]
        public void TestTooManyGroups()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g1", "g1_threeBatch.txt", 7));
        }

        [Fact]
        public void TestIncompleteGroups()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g1", "g1_missingBatchTrailer.txt", 12));
        }

        [Fact]
        public void TestUnidentifiedRecord()
        {
            Assert.Throws<UnidentifiedRecordException>(() => Test("g1", "g1_unidentifiedRecord.txt", 8));
        }

        [Fact]
        public void TestOrderChoice1()
        {
            Test("g2", "g2_valid1.txt");
        }

        [Fact]
        public void TestTooManyGroups2()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g2", "g2_tooManyGroups.txt", 15));
        }

        [Fact]
        public void TestMissingGroup()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g2", "g2_missingGroup.txt", 11));
        }

        [Fact]
        public void TestStreamMinOccurs()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g2", "g2_empty.txt", -1));
        }

        [Fact]
        public void TestStreamMaxOccurs()
        {
            Test("g2", "g2_twoLayout.txt");
        }

        [Fact]
        public void TestUnorderedMaxOccurs()
        {
            Test("g3", "g3_valid.txt");
        }

        [Fact]
        public void TestRepeatingGroupWithOptionalHeader()
        {
            Test("g4", "g4_repeatingGroup.txt");
        }

        [Fact]
        public void TestEndOfGroup()
        {
            Test("g4", "g4_endOfGroup.txt");
        }

        [Fact]
        public void TestTooManyGroupsWithOptionalRecords()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g4", "g4_tooManyGroups.txt", 8));
        }

        [Fact]
        public void TestSubgroups()
        {
            Test("g5", "g5_subgroups.txt");
        }

        [Fact]
        public void TestMissingRecordEOF()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g6", "g6_missingRecordEOF.txt"));
        }

        [Fact]
        public void TestMissingGroupEOF()
        {
            Assert.Throws<UnexpectedRecordException>(() => Test("g7", "g7_missingGroupEOF.txt"));
        }

        [Fact]
        public void TestRepeatingStream()
        {
            Test("g8", "g8_repeatingStream.txt");
        }

        /// <summary>
        /// Fully parses the given file.
        /// </summary>
        /// <param name="name">the name of the stream</param>
        /// <param name="fileName">the name of the file to test</param>
        private void Test(string name, string fileName)
        {
            Test(name, fileName, -1);
        }

        /// <summary>
        /// Fully parses the given file.
        /// </summary>
        /// <param name="name">the name of the stream</param>
        /// <param name="fileName">the name of the file to test</param>
        /// <param name="errorLineNumber">the error line number</param>
        private void Test(string name, string fileName, int errorLineNumber)
        {
            var factory = NewStreamFactory("group.xml");
            var reader = factory.CreateReader(name, LoadReader(fileName));
            try
            {
                while (reader.Read() != null)
                {
                }
            }
            catch (BeanReaderException ex)
            {
                if (errorLineNumber > 0)
                {
                    // assert the line number from the exception matches expected
                    Assert.Equal(errorLineNumber, ex.RecordContext.LineNumber);
                }

                throw;
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
