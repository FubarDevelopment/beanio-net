// <copyright file="TypeUtilTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

using NodaTime;

using Xunit;

namespace BeanIO.Internal.Util
{
    public class TypeUtilTest
    {
        [Fact]
        public void TestToType()
        {
            Assert.Equal(typeof(string), "string".ToType());
            Assert.Equal(typeof(bool), "bool".ToType());
            Assert.Equal(typeof(byte), "byte".ToType());
            Assert.Equal(typeof(sbyte), "sbyte".ToType());
            Assert.Equal(typeof(char), "char".ToType());
            Assert.Equal(typeof(char), "character".ToType());
            Assert.Equal(typeof(short), "short".ToType());
            Assert.Equal(typeof(ushort), "ushort".ToType());
            Assert.Equal(typeof(int), "int".ToType());
            Assert.Equal(typeof(int), "integer".ToType());
            Assert.Equal(typeof(uint), "uint".ToType());
            Assert.Equal(typeof(long), "long".ToType());
            Assert.Equal(typeof(ulong), "ulong".ToType());
            Assert.Equal(typeof(float), "float".ToType());
            Assert.Equal(typeof(double), "double".ToType());
            Assert.Equal(typeof(decimal), "decimal".ToType());
            Assert.Equal(typeof(DateTime), "datetime".ToType());
            Assert.Equal(typeof(DateTimeOffset), "datetimeoffset".ToType());
            Assert.Equal(typeof(LocalDate), "date".ToType());
            Assert.Equal(typeof(LocalTime), "time".ToType());
            Assert.Equal(GetType(), "BeanIO.Internal.Util.TypeUtilTest, BeanIO.Test".ToType());
            Assert.Equal(typeof(List<>), "System.Collections.Generic.List`1".ToType());
            Assert.Equal(typeof(IList<>), "System.Collections.Generic.IList`1".ToType());
        }

        [Fact]
        public void TestToTypeClassNotFound()
        {
            Assert.Null("BeanIO.Types.NoClass".ToType());
        }

        [Fact]
        public void TestToAggregation()
        {
            Assert.Equal(typeof(IList), "list".ToAggregationType(null));
            Assert.Equal(typeof(IList), "collection".ToAggregationType(null));
            Assert.Equal(typeof(ISet<object>), "set".ToAggregationType(null));
            Assert.Equal(typeof(Array), "array".ToAggregationType(null));
            Assert.Equal(typeof(IList<>), "System.Collections.Generic.IList`1".ToAggregationType(null));
            Assert.Equal(typeof(ArrayList).FullName, "System.Collections.ArrayList".ToAggregationType(null).FullName);
            Assert.Equal(typeof(IDictionary<,>), "map".ToAggregationType(null));
            Assert.Equal(typeof(IDictionary<,>), "System.Collections.Generic.IDictionary`2".ToAggregationType(null));
            Assert.Null("BeanIO.Types.NoClass".ToAggregationType(null));
        }
    }
}
