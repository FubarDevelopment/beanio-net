// <copyright file="XmlMultilineRecordTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Xml.Multiline
{
    public sealed class XmlMultilineRecordTest : XmlParserTest
    {
        private readonly StreamFactory _factory;

        public XmlMultilineRecordTest()
        {
            _factory = NewStreamFactory("xml_multiline_mapping.xml");
        }

        [Fact]
        public void TestRecordGroup()
        {
            var reader = _factory.CreateReader("ml1", LoadReader("ml1_in.xml"));

            try
            {
                // Read a valid multi-line record
                Beans.Order order = Assert.IsType<Beans.Order>(reader.Read());

                Assert.Equal(3, reader.LineNumber);
                Assert.Equal(4, reader.RecordCount);
                Assert.Equal("orderGroup", reader.RecordName);

                var ctx = reader.GetRecordContext(1);
                Assert.Equal(7, ctx.LineNumber);
                Assert.Equal("customer", ctx.RecordName);

                Assert.Equal("100", order.Id);
                Assert.Equal(new DateTime(2012, 1, 1), order.Date);

                var buyer = order.Customer;
                Assert.NotNull(buyer);
                Assert.Equal("George", buyer.FirstName);
                Assert.Equal("Smith", buyer.LastName);

                Assert.NotNull(order.Items);
                Assert.Collection(
                    order.Items,
                    item =>
                        {
                            Assert.Equal("soda", item.Name);
                            Assert.Equal(2, item.Quantity);
                        },
                    item =>
                        {
                            Assert.Equal("carrots", item.Name);
                            Assert.Equal(5, item.Quantity);
                        });

                var text = new StringWriter();
                var writer = _factory.CreateWriter("ml1", text);
                writer.Write(order);

                // Write bean object with missing data
                order.Customer = null;
                order.Items = null;
                writer.Write(order);

                // Read an invalid multi-line record
                var ex = Assert.ThrowsAny<InvalidRecordException>(() => reader.Read());
                Assert.Equal(20, reader.LineNumber);
                Assert.Equal(2, reader.RecordCount);
                Assert.Equal("orderGroup", reader.RecordName);

                ctx = ex.RecordContexts[1];
                Assert.True(ctx.HasFieldErrors);
                Assert.Equal(24, ctx.LineNumber);
                Assert.Equal("item", ctx.RecordName);
                Assert.Equal("a", ctx.GetFieldText("quantity", 0));

                // Skip 2 invalid records
                Assert.Equal(2, reader.Skip(2));

                // Read another valid record
                var order2 = (Beans.Order?)reader.Read();
                Assert.NotNull(order2);
                Assert.Equal(55, reader.LineNumber);
                Assert.Equal(3, reader.RecordCount);
                Assert.Equal("orderGroup", reader.RecordName);
                Assert.Equal("104", order2.Id);
                Assert.Null(order2.Customer);

                writer.Write(order2);
                writer.Flush();
                writer.Close();

                Assert.Equal(Load("ml1_out.xml"), text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
