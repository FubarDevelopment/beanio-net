// <copyright file="RidLengthTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

using Xunit;

namespace BeanIO.Parser.RidLength
{
    public class RidLengthTest : ParserTest
    {
        [Fact]
        public void TestRidLength()
        {
            var factory = NewStreamFactory("BeanIO.Parser.RidLength.ridlength_mapping.xml");
            var reader = factory.CreateReader("r1", LoadReader("r1.txt"));
            try
            {
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal("acouple", reader.RecordName);
                Assert.True(map.ContainsKey("values"));
                Assert.Equal(new[] { 1, 2 }, Assert.IsType<List<int>>(map["values"]));

                map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.Equal("afew", reader.RecordName);
                Assert.True(map.ContainsKey("values"));
                Assert.Equal(new[] { 1, 2, 3 }, Assert.IsType<List<int>>(map["values"]));

                Assert.IsType(typeof(Dictionary<string, object>), reader.Read());
                Assert.Equal("acouple", reader.RecordName);

                Assert.IsType(typeof(Dictionary<string, object>), reader.Read());
                Assert.Equal("afew", reader.RecordName);
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
