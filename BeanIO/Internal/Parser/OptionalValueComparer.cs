﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace BeanIO.Internal.Parser
{
    public sealed class OptionalValueComparer : IComparer<OptionalValue>, IEqualityComparer<OptionalValue>, IComparer, IEqualityComparer
    {
        private static readonly OptionalValueComparer _default = new OptionalValueComparer(StringComparer.Ordinal);

        private static readonly OptionalValueComparer _ignoreCase = new OptionalValueComparer(StringComparer.OrdinalIgnoreCase);

        private readonly StringComparer _stringComparer;

        private OptionalValueComparer(StringComparer comparer)
        {
            _stringComparer = comparer;
        }

        public static OptionalValueComparer Default
        {
            get { return _default; }
        }

        public static OptionalValueComparer IgnoreCase
        {
            get { return _ignoreCase; }
        }

        /// <summary>
        /// Compares to objects
        /// </summary>
        /// <returns>0, if equal, &lt;0 if less and &gt;0 if greater</returns>
        /// <param name="x">The value to compare with.</param>
        /// <param name="y">The value to compare to.</param>
        public int Compare(OptionalValue x, OptionalValue y)
        {
            if (ReferenceEquals(x, y))
                return 0;

            if (ReferenceEquals(x, null))
                return -1;

            if (ReferenceEquals(y, null))
                return 1;

            var result = x.CompareStatus(y);
            if (result != 0)
                return result;

            if (!x.HasText)
                return 0;

            return _stringComparer.Compare(x.Text ?? string.Empty, y.Text ?? string.Empty);
        }

        /// <summary>
        /// Determines whether both objects are equal
        /// </summary>
        /// <returns>true, when both objects are equal, false otherwise</returns>
        /// <param name="x">The value to compare with.</param>
        /// <param name="y">The value to compare to.</param>
        public bool Equals(OptionalValue x, OptionalValue y)
        {
            return Compare(x, y) == 0;
        }

        /// <summary>
        /// Returns the hash code for the given object.
        /// </summary>
        /// <returns>
        /// The hash code for the given object.
        /// </returns>
        /// <param name="obj">The <see cref="OptionalValue"/> to return the hash code for</param>
        public int GetHashCode(OptionalValue obj)
        {
            var result = obj.StatusHashCode;
            if (obj.HasText)
                result ^= (obj.Text ?? string.Empty).GetHashCode();
            return result;
        }

        int IComparer.Compare(object x, object y)
        {
            return Compare((OptionalValue)x, (OptionalValue)y);
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            return Equals((OptionalValue)x, (OptionalValue)y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return GetHashCode((OptionalValue)obj);
        }
    }
}