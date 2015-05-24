using System;

using BeanIO.Internal.Parser;

using Xunit;

namespace BeanIO.Test
{
    public class OptionalValueTests
    {
        [Fact]
        public void OptionalValueEquality()
        {
            var missing = OptionalValue.Missing;
            var invalid = OptionalValue.Invalid;
            var nil = OptionalValue.Nil;

            Assert.Equal(missing, OptionalValue.Missing, OptionalValueComparer.Default);
            Assert.NotEqual(missing, OptionalValue.Invalid, OptionalValueComparer.Default);
            Assert.NotEqual(missing, OptionalValue.Nil, OptionalValueComparer.Default);

            Assert.NotEqual(invalid, OptionalValue.Missing, OptionalValueComparer.Default);
            Assert.Equal(invalid, OptionalValue.Invalid, OptionalValueComparer.Default);
            Assert.NotEqual(invalid, OptionalValue.Nil, OptionalValueComparer.Default);

            Assert.NotEqual(nil, OptionalValue.Missing, OptionalValueComparer.Default);
            Assert.NotEqual(nil, OptionalValue.Invalid, OptionalValueComparer.Default);
            Assert.Equal(nil, OptionalValue.Nil, OptionalValueComparer.Default);

            Assert.True(OptionalValueComparer.Default.Equals(string.Empty, new OptionalValue(null)));
            Assert.True(OptionalValueComparer.Default.Equals(new OptionalValue(null), string.Empty));

            Assert.False(OptionalValueComparer.Default.Equals(string.Empty, OptionalValue.Missing));
            Assert.False(OptionalValueComparer.Default.Equals(string.Empty, OptionalValue.Invalid));
            Assert.False(OptionalValueComparer.Default.Equals(string.Empty, OptionalValue.Nil));

            Assert.True(missing == OptionalValue.Missing);
            Assert.False(missing == OptionalValue.Invalid);
            Assert.False(missing == OptionalValue.Nil);

            Assert.False(invalid == OptionalValue.Missing);
            Assert.True(invalid == OptionalValue.Invalid);
            Assert.False(invalid == OptionalValue.Nil);

            Assert.False(nil == OptionalValue.Missing);
            Assert.False(nil == OptionalValue.Invalid);
            Assert.True(nil == OptionalValue.Nil);

            Assert.True(string.Empty == new OptionalValue(null));
            Assert.True(new OptionalValue(null) == string.Empty);

            Assert.True(new OptionalValue(null) == new OptionalValue(string.Empty));
            Assert.True(new OptionalValue(string.Empty) == new OptionalValue(null));

            Assert.False(string.Empty == OptionalValue.Missing);
            Assert.False(string.Empty == OptionalValue.Invalid);
            Assert.False(string.Empty == OptionalValue.Nil);

            Assert.True(string.Empty != OptionalValue.Missing);

            Assert.Equal(new OptionalValue("a"), new OptionalValue("A"), OptionalValueComparer.IgnoreCase);
            Assert.NotEqual(new OptionalValue("a"), new OptionalValue("A"), OptionalValueComparer.Default);
        }
    }
}
