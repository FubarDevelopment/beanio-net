// <copyright file="IgnoreUnidentifiedRecordsParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using Xunit;

namespace BeanIO.Parser.IgnoreUnidentifiedRecords
{
    public class IgnoreUnidentifiedRecordsParserTest : ParserTest
    {
        [Fact]
        public void TestIgnoreUnidentifiedRecords()
        {
            var factory = NewStreamFactory("ignore_mapping.xml");
            var reader = factory.CreateReader("stream1", LoadReader("ignoreUnidentifiedRecords1.txt"));
            try
            {
                reader.Read();
                Assert.Equal("header", reader.RecordName);
                reader.Read();
                Assert.Equal("group_header", reader.RecordName);
                reader.Read();
                Assert.Equal("group_trailer", reader.RecordName);

                reader.Read();
                Assert.Equal("header", reader.RecordName);
                reader.Read();
                Assert.Equal("group_header", reader.RecordName);
                reader.Read();
                Assert.Equal("group_trailer", reader.RecordName);

                Assert.Null(reader.Read());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
