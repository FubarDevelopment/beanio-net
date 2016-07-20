// <copyright file="MultilineRecordTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;

using BeanIO.Beans;

using Xunit;

namespace BeanIO.Parser.Multiline
{
    public class MultilineRecordTest : ParserTest
    {
        [Fact]
        public void TestRecordGroup()
        {
            var factory = NewStreamFactory("multiline_mapping.xml");
            var reader = factory.CreateReader("ml1", LoadReader("ml1.txt"));
            try
            {
                // read a valid multi-line record
                var order = Assert.IsType<Beans.Order>(reader.Read());

                Assert.Equal(1, reader.LineNumber);
                Assert.Equal(4, reader.RecordCount);
                Assert.Equal("orderGroup", reader.RecordName);

                var ctx = reader.GetRecordContext(1);
                Assert.Equal(2, ctx.LineNumber);
                Assert.Equal("customer", ctx.RecordName);
                Assert.Equal("customer,George,Smith", ctx.RecordText);

                Assert.Equal("100", order.Id);
                Assert.Equal(new DateTime(2012, 1, 1), order.Date);

                var buyer = order.Customer;
                Assert.NotNull(buyer);
                Assert.Equal("George", buyer.FirstName);
                Assert.Equal("Smith", buyer.LastName);

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
                factory.CreateWriter("ml1", text).Write(order);
                Assert.Equal(
                    "order,100,2012-01-01\n" +
                    "customer,George,Smith\n" +
                    "item,soda,2\n" +
                    "item,carrots,5\n",
                    text.ToString());

                order.Customer = null;
                order.Items = null;

                text = new StringWriter();
                factory.CreateWriter("ml1", text).Write(order);
                Assert.Equal(
                    "order,100,2012-01-01\n" +
                    "item,,\n",
                    text.ToString());

                // read an invalid multi-line record
                var ex = Assert.Throws<InvalidRecordGroupException>(() => reader.Read());
                Assert.Equal(5, reader.LineNumber);
                Assert.Equal(2, reader.RecordCount);
                Assert.Equal("orderGroup", reader.RecordName);

                ctx = ex.RecordContexts[1];
                Assert.Equal(6, ctx.LineNumber);
                Assert.Equal("item", ctx.RecordName);
                Assert.Equal("a", ctx.GetFieldText("quantity", 0));

                // skip an invalid record
                Assert.Equal(2, reader.Skip(2));

                // read another valid record
                order = Assert.IsType<Beans.Order>(reader.Read());
                Assert.Equal(13, reader.LineNumber);
                Assert.Equal(3, reader.RecordCount);
                Assert.Equal("orderGroup", reader.RecordName);
                Assert.Equal("103", order.Id);
                Assert.Null(order.Customer);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestNestedRecordGroup()
        {
            var factory = NewStreamFactory("multiline_mapping.xml");
            var reader = factory.CreateReader("ml2", LoadReader("ml2.txt"));
            try
            {
                // read batch #1
                var batch = Assert.IsType<OrderBatch>(reader.Read());
                Assert.Equal(2, batch.BatchCount);

                var orderList = batch.Orders;
                Assert.Collection(
                    orderList,
                    order =>
                        {
                            Assert.Equal("100", order.Id);
                            Assert.NotNull(order.Customer);
                            Assert.Equal("George", order.Customer.FirstName);
                            Assert.Equal("Smith", order.Customer.LastName);
                        },
                    order =>
                        {
                            Assert.Equal("101", order.Id);
                            Assert.NotNull(order.Customer);
                            Assert.Equal("Joe", order.Customer.FirstName);
                            Assert.Equal("Johnson", order.Customer.LastName);
                        });

                // read batch #2
                batch = Assert.IsType<OrderBatch>(reader.Read());
                Assert.Equal(1, batch.BatchCount);
                Assert.Equal("103", batch.Orders[0].Id);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestNestedRecordGroupCollections()
        {
            var factory = NewStreamFactory("multiline_mapping.xml");
            var reader = factory.CreateReader("ml3", LoadReader("ml3.txt"));
            try
            {
                // read batch #1
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                Assert.True(map.ContainsKey("batch"));
                var list = Assert.IsType<List<OrderBatch>>(map["batch"]);
                Assert.Collection(
                    list,
                    orderList =>
                        {
                            Assert.Collection(
                                orderList.Orders,
                                order => Assert.Equal("100", order.Id),
                                order => Assert.Equal("101", order.Id));
                        },
                    orderList =>
                        {
                            Assert.Collection(
                                orderList.Orders,
                                order => Assert.Equal("103", order.Id));
                        });

                var text = new StringWriter();
                factory.CreateWriter("ml3", text).Write(map);
                Assert.Equal(
                    "header,2\n" +
                    "order,100,2012-01-01\n" +
                    "customer,George,Smith\n" +
                    "order,101,2012-01-01\n" +
                    "customer,John,Smith\n" +
                    "header,1\n" +
                    "order,103,2012-01-01\n" +
                    "customer,Jen,Smith\n",
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestRecordMap()
        {
            var factory = NewStreamFactory("multiline_mapping.xml");
            var reader = factory.CreateReader("ml4", LoadReader("ml4.txt"));
            try
            {
                // read order #1
                var order = Assert.IsType<Beans.Order>(reader.Read());

                var itemMap = order.ItemMap;
                Assert.Collection(
                    itemMap,
                    item =>
                        {
                            Assert.Equal("soda", item.Key);
                            Assert.Equal("soda", item.Value.Name);
                            Assert.Equal(2, item.Value.Quantity);
                        },
                    item =>
                        {
                            Assert.Equal("carrots", item.Key);
                            Assert.Equal("carrots", item.Value.Name);
                            Assert.Equal(5, item.Value.Quantity);
                        });

                var text = new StringWriter();
                var writer = factory.CreateWriter("ml4", text);

                writer.Write(order);

                order = Assert.IsType<Beans.Order>(reader.Read());
                Assert.NotNull(order.ItemMap);
                Assert.Equal(3, order.ItemMap.Count);

                writer.Write(order);

                writer.Flush();

                Assert.Equal(
                    "order,100,2012-01-01\n" +
                    "item,soda,2\n" +
                    "item,carrots,5\n" +
                    "order,101,2012-01-01\n" +
                    "item,banana,1\n" +
                    "item,apple,2\n" +
                    "item,cereal,3\n",
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestNestedRecordGroupNonCollection()
        {
            var factory = NewStreamFactory("multiline_mapping.xml");
            var reader = factory.CreateReader("ml5", LoadReader("ml5.txt"));
            try
            {
                var batch = Assert.IsType<OrderBatch>(reader.Read());
                Assert.Equal(2, batch.BatchCount);

                var order = batch.Order;
                Assert.NotNull(order);
                Assert.Equal("100", order.Id);

                var customer = order.Customer;
                Assert.NotNull(customer);
                Assert.Equal("George", customer.FirstName);

                var text = new StringWriter();
                factory.CreateWriter("ml5", text).Write(batch);
                Assert.Equal(
                    "header,2\n" +
                    "order,100,2012-01-01\n" +
                    "customer,George,Smith\n",
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestEmptyRecordList()
        {
            var factory = NewStreamFactory("multiline_mapping.xml");
            var reader = factory.CreateReader("ml6", LoadReader("ml6.txt"));
            try
            {
                // read a valid multi-line record
                var order = Assert.IsType<Beans.Order>(reader.Read());
                Assert.Equal(1, reader.LineNumber);
                Assert.Equal(1, reader.RecordCount);
                Assert.Equal("orderGroup", reader.RecordName);

                Assert.Null(order.Items);
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestInlineRecordMap()
        {
            var factory = NewStreamFactory("multiline_mapping.xml");
            var reader = factory.CreateReader("ml7", LoadReader("ml7.txt"));
            try
            {
                var record = Assert.IsType<Dictionary<string, string>>(reader.Read());
                Assert.Collection(
                    record,
                    item =>
                        {
                            Assert.Equal("key1", item.Key);
                            Assert.Equal("value1", item.Value);
                        },
                    item =>
                        {
                            Assert.Equal("key2", item.Key);
                            Assert.Equal("value2", item.Value);
                        },
                    item =>
                        {
                            Assert.Equal("key3", item.Key);
                            Assert.Equal("value3", item.Value);
                        });
                var text = new StringWriter();
                factory.CreateWriter("ml7", text).Write(record);
                Assert.Equal(
                    "key1,value1\n" +
                    "key2,value2\n" +
                    "key3,value3\n",
                    text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestOptionalRecord()
        {
            var factory = NewStreamFactory("multiline_mapping.xml");
            var text = "CUSTGeorge" + LineSeparator;
            var reader = factory.CreateReader("ml8", new StringReader(text));
            var order = Assert.IsType<Beans.Order>(reader.Read());
            Assert.NotNull(order.Customer);
            Assert.Equal("CUST", order.Customer.Id);
            Assert.Equal("George", order.Customer.FirstName);
            Assert.Null(order.Shipper);

            var output = new StringWriter();
            factory.CreateWriter("ml8", output).Write(order);
            Assert.Equal(text, output.ToString());
        }
    }
}
