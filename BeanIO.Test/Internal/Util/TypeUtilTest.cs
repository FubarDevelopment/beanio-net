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
            Assert.Equal(typeof(List<>), "list".ToAggregationType());
            Assert.Equal(typeof(List<>), "collection".ToAggregationType());
            Assert.Equal(typeof(HashSet<>), "set".ToAggregationType());
            Assert.Equal(typeof(Array), "array".ToAggregationType());
            Assert.Equal(typeof(List<>), "System.Collections.Generic.List`1".ToAggregationType());
            Assert.Equal(typeof(ArrayList), "System.Collections.ArrayList".ToAggregationType());
            Assert.Equal(typeof(Dictionary<,>), "map".ToAggregationType());
            Assert.Equal(typeof(Dictionary<,>), "System.Collections.Generic.Dictionary`2".ToAggregationType());
            Assert.Null("BeanIO.Types.NoClass".ToAggregationType());
        }
    }
}
