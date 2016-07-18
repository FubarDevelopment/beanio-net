// <copyright file="ImportParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;

using NodaTime;

using Xunit;

namespace BeanIO.Parser.Imports
{
    public class ImportParserTest : ParserTest
    {
        [Fact]
        public void TestImportHierarchy()
        {
            var factory = NewStreamFactory("FubarDev.BeanIO.Test.Parser.Imports.import_mapping1.xml");
            var reader = factory.CreateReader("stream1.1", new StringReader("Joe ,1970-01-01"));
            var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
            Assert.True(map.ContainsKey("name"));
            Assert.Equal("Joe", map["name"]);
            Assert.True(map.ContainsKey("date"));
            Assert.Equal(new LocalDate(1970, 1, 1), map["date"]);

            reader = factory.CreateReader("stream2.1", new StringReader("Joe ,01021970"));
            map = Assert.IsType<Dictionary<string, object>>(reader.Read());
            Assert.True(map.ContainsKey("name"));
            Assert.Equal("Joe ", map["name"]);
            Assert.True(map.ContainsKey("date"));
            Assert.Equal(new LocalDate(1970, 1, 2), map["date"]);

            reader = factory.CreateReader("stream3.1", new StringReader("Joe,01021970"));
            map = Assert.IsType<Dictionary<string, object>>(reader.Read());
            Assert.True(map.ContainsKey("name"));
            Assert.Equal("Joe", map["name"]);
            Assert.True(map.ContainsKey("date"));
            Assert.Equal(new LocalDate(1970, 1, 2), map["date"]);
        }

        [Fact]
        public void TestCircularReference()
        {
            var ex = Assert.Throws<BeanIOConfigurationException>(() => NewStreamFactory("FubarDev.BeanIO.Test.Parser.Imports.circular_mapping1.xml"));
            Assert.Equal(
                "Invalid mapping file 'resource:FubarDev.BeanIO.Test.Parser.Imports.circular_mapping1.xml, FubarDev.BeanIO.Test': " +
                "Failed to import resource 'resource:FubarDev.BeanIO.Test.Parser.Imports.circular_mapping2.xml, FubarDev.BeanIO.Test': " +
                "Circular reference(s) detected",
                ex.Message);
        }
    }
}
