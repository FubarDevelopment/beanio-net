// <copyright file="OrdinalTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Annotation;
using BeanIO.Builder;

using Xunit;

namespace BeanIO.Parser.Order
{
    public class OrdinalTest
    {
        [Fact]
        public void TestOrdinal()
        {
            var factory = StreamFactory.NewInstance();
            factory.Define(
                new StreamBuilder("c1")
                    .Format("csv")
                    .AddRecord(typeof(Man)));

            var m = factory.CreateMarshaller("c1");

            var man = new Man()
                {
                    Age = 15,
                    LastName = "jones",
                    FirstName = "jason",
                    Company = "apple",
                    Ext = "1234",
                };

            Assert.Equal("jason,jones,15,apple,1234", m.Marshal(man).ToString());
        }

        [Record]
        public class Man
        {
            [Field]
            public string? Company { get; set; }

            [Field(Ordinal = 3)]
            public int Age { get; set; }

            [Field]
            public string? Ext { get; set; }

            [Field(Ordinal = 2)]
            public string? LastName { get; set; }

            [Field(Ordinal = 1)]
            public string? FirstName { get; set; }
        }
    }
}
