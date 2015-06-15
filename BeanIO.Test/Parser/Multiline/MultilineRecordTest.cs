using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var factory = NewStreamFactory("BeanIO.Parser.Multiline.multiline_mapping.xml");
            var reader = factory.CreateReader("ml1", LoadStream("ml1.txt"));
            try
            {
                // read a valid multi-line record
                var order = Assert.IsType<Order>(reader.Read());

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
                order = Assert.IsType<Order>(reader.Read());
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
        public void TestNestedRecorGroup()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Multiline.multiline_mapping.xml");
            var reader = factory.CreateReader("ml2", LoadStream("ml2.txt"));
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
        public void TestNestedRecorGroupCollections()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Multiline.multiline_mapping.xml");
            var reader = factory.CreateReader("ml3", LoadStream("ml3.txt"));
            try
            {
                // read batch #1
                var map = Assert.IsType<Dictionary<string, object>>(reader.Read());
                throw new NotImplementedException();
            }
            finally
            {
                reader.Close();
            }
        }

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.Multiline.{0}", fileName);
            var asm = typeof(MultilineRecordTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            Debug.Assert(resStream != null, "resStream != null");
            return new StreamReader(resStream);
        }
    }
}
