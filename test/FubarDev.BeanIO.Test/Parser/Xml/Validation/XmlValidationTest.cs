// <copyright file="XmlValidationTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using Xunit;

namespace BeanIO.Parser.Xml.Validation
{
    public sealed class XmlValidationTest : XmlParserTest
    {
        private readonly StreamFactory _factory;

        public XmlValidationTest()
        {
            _factory = NewStreamFactory("validation_mapping.xml");
        }

        [Fact]
        public void TestFieldErrorsNillableAndMinOccurs()
        {
            var reader = _factory.CreateReader("stream", LoadReader("v1_in.xml"));
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.Equal(string.Empty, person.FirstName);
                Assert.Equal(string.Empty, person.LastName);

                AssertFieldError(reader, 6, "person", "firstName", 0, null, "Field is not nillable");
                AssertFieldError(reader, 10, "person", "lastName", 0, null, "Field is not nillable");
                AssertFieldError(reader, 14, "person", "firstName", 0, null, "Expected minimum 1 occurrences");
                AssertFieldError(reader, 17, "person", "lastName", 0, null, "Expected minimum 1 occurrences");
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestFieldErrorsNillableBean()
        {
            var reader = _factory.CreateReader("stream2", LoadReader("v2_in.xml"));
            try
            {
                var person = Assert.IsType<Person>(reader.Read());
                Assert.NotNull(person.Address);
                Assert.Equal("IL", person.Address.State);

                AssertFieldError(reader, 7, "person", "address", 0, null, "Field is not nillable");
                AssertFieldError(reader, 10, "person", "address", 0, null, "Expected minimum 1 occurrences");
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
