// <copyright file="XmlCollectionTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Xml.Collection
{
    public sealed class XmlCollectionTest : XmlParserTest
    {
        private readonly IStreamFactory _factory;

        public XmlCollectionTest()
        {
            _factory = NewStreamFactory("collection_mapping.xml");
        }

        /// <summary>
        /// Test an XML field collection.
        /// </summary>
        [Fact]
        public void TestFieldCollection()
        {
            var reader = _factory.CreateReader("stream", LoadReader("c1_in.xml"));

            var s = new StringWriter();
            var writer = _factory.CreateWriter("stream", s);
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("John", person.FirstName);
                var list = person.Color;
                Assert.Collection(
                    person.Color,
                    item => Assert.Equal("Red", item),
                    item => Assert.Equal("Blue", item),
                    item => Assert.Equal("Green", item));
                writer.Write(person);

                person = Assert.IsType<Person>(reader.Read());
                Assert.Equal("George", person.FirstName);
                Assert.Empty(person.Color);
                writer.Write(person);

                writer.Close();
                Assert.Equal(Load("c1_in.xml"), s.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
