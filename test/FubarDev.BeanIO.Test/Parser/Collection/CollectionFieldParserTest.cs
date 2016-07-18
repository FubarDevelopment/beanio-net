// <copyright file="CollectionFieldParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

using Xunit;

namespace BeanIO.Parser.Collection
{
    public class CollectionFieldParserTest : ParserTest
    {
        [Fact]
        public void TestCollectionDelimited()
        {
            var factory = NewStreamFactory("FubarDev.BeanIO.Test.Parser.Collection.collection.xml");
            var reader = factory.CreateReader("dc1", LoadReader("dc1_valid.txt"));
            try
            {
                var bean = (CollectionBean)reader.Read();
                Assert.Equal(new[] { "George", "Gary", "Jon" }, bean.List);
                Assert.Equal(new[] { 1, 2, 3, 4 }, bean.Array);

                var text = new StringWriter();
                factory.CreateWriter("dc1", text).Write(bean);
                Assert.Equal("George,Gary,Jon,1,2,3,4" + LineSeparator, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestNullPrimitive()
        {
            var factory = NewStreamFactory("FubarDev.BeanIO.Test.Parser.Collection.collection.xml");
            var reader = factory.CreateReader("dc2", LoadReader("dc2_nullPrimitive.txt"));
            try
            {
                var bean = (CollectionBean)reader.Read();
                Assert.Equal(new[] { 1, 0, 3 }, bean.Array);

                var text = new StringWriter();
                factory.CreateWriter("dc2", text).Write(new CollectionBean());
                Assert.Equal(",," + LineSeparator, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestCollectionFixedLength()
        {
            var factory = NewStreamFactory("FubarDev.BeanIO.Test.Parser.Collection.collection.xml");
            var reader = factory.CreateReader("fc1", LoadReader("fc1_valid.txt"));
            try
            {
                var bean = (CollectionBean)reader.Read();
                Assert.Equal(new[] { 1, 100, 24 }, bean.Array);
                Assert.Equal(new char?[] { 'A', 'B', 'C', ' ' }, bean.Set);

                var text = new StringWriter();
                factory.CreateWriter("fc1", text).Write(bean);
                Assert.Equal("001100024ABC " + LineSeparator, text.ToString());

                bean = (CollectionBean)reader.Read();
                Assert.Equal(new[] { 0, 400, 500 }, bean.Array);
                Assert.Empty(bean.Set);

                text = new StringWriter();
                factory.CreateWriter("fc1", text).Write(bean);
                Assert.Equal("000400500" + LineSeparator, text.ToString());

                text = new StringWriter();
                factory.CreateWriter("fc1", text).Write(new CollectionBean());
                Assert.Equal("000000000" + LineSeparator, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }

        [Fact]
        public void TestCollectionFixedLengthWithDerivedClass()
        {
            var factory = NewStreamFactory("FubarDev.BeanIO.Test.Parser.Collection.collection.xml");
            var reader = factory.CreateReader("fc2", LoadReader("fc2_valid.txt"));
            try
            {
                StringWriter text;
                var bean = (CollectionBean)reader.Read();
                Assert.Collection(
                    bean.ObjectList,
                    person =>
                    {
                        var dp = Assert.IsType<DerivedPerson>(person);
                        Assert.Equal("firs1", dp.FirstName);
                        Assert.Equal("last1", dp.LastName);
                        Assert.Equal("nick1", dp.NickName);
                    });
                text = new StringWriter();
                factory.CreateWriter("fc2", text).Write(bean);
                Assert.Equal("01firs1last1nick1" + LineSeparator, text.ToString());

                bean = (CollectionBean)reader.Read();
                Assert.Collection(
                    bean.ObjectList,
                    person =>
                    {
                        var dp = Assert.IsType<DerivedPerson>(person);
                        Assert.Equal("firs2", dp.FirstName);
                        Assert.Equal("last2", dp.LastName);
                        Assert.Equal("nick2", dp.NickName);
                    },
                    person =>
                    {
                        var dp = Assert.IsType<DerivedPerson>(person);
                        Assert.Equal("firs3", dp.FirstName);
                        Assert.Equal("last3", dp.LastName);
                        Assert.Equal("nick3", dp.NickName);
                    });
                text = new StringWriter();
                factory.CreateWriter("fc2", text).Write(bean);
                Assert.Equal("02firs2last2nick203firs3last3nick3" + LineSeparator, text.ToString());
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
