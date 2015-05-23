using System;

namespace BeanIO.Internal.Parser
{
    public sealed class OptionalValue : IComparable<OptionalValue>, IEquatable<OptionalValue>, IComparable
    {
        /// <summary>
        /// Constant indicating the field did not pass validation
        /// </summary>
        public static readonly OptionalValue Invalid = new OptionalValue(Status.Invalid);

        /// <summary>
        /// Constant indicating the field was not present in the stream
        /// </summary>
        public static readonly OptionalValue Missing = new OptionalValue(Status.Missing);

        /// <summary>
        /// Constant indicating the field was nil (XML only)
        /// </summary>
        public static readonly OptionalValue Nil = new OptionalValue(Status.Nil);

        private readonly Status _status;

        private readonly string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalValue"/> class.
        /// </summary>
        /// <param name="text">The text value to initialize the <see cref="OptionalValue"/> with.</param>
        public OptionalValue(string text)
        {
            _value = text;
            _status = Status.HasValue;
        }

        private OptionalValue(Status status)
        {
            _status = status;
        }

        private enum Status
        {
            Invalid,
            Missing,
            Nil,
            HasValue
        }

        public static implicit operator OptionalValue(string text)
        {
            return new OptionalValue(text);
        }

        public static bool operator ==(OptionalValue value1, OptionalValue value2)
        {
            return OptionalValueComparer.Default.Equals(value1, value2);
        }

        public static bool operator !=(OptionalValue value1, OptionalValue value2)
        {
            return !OptionalValueComparer.Default.Equals(value1, value2);
        }

        public static bool operator ==(OptionalValue value1, object value2)
        {
            return ReferenceEquals(value1, value2);
        }

        public static bool operator !=(OptionalValue value1, object value2)
        {
            return !ReferenceEquals(value1, value2);
        }

        public static bool operator <(OptionalValue value1, OptionalValue value2)
        {
            return OptionalValueComparer.Default.Compare(value1, value2) < 0;
        }

        public static bool operator >(OptionalValue value1, OptionalValue value2)
        {
            return OptionalValueComparer.Default.Compare(value1, value2) > 0;
        }

        public static bool operator <=(OptionalValue value1, OptionalValue value2)
        {
            return OptionalValueComparer.Default.Compare(value1, value2) <= 0;
        }

        public static bool operator >=(OptionalValue value1, OptionalValue value2)
        {
            return OptionalValueComparer.Default.Compare(value1, value2) >= 0;
        }

        public bool IsInvalid => _status == Status.Invalid;

        public bool IsMissing => _status == Status.Missing;

        public bool IsNil => _status == Status.Nil;

        public bool HasValue => _status == Status.HasValue;

        public string Text
        {
            get
            {
                if (!HasValue)
                    throw new InvalidOperationException();
                return _value;
            }
        }

        /// <summary>
        /// Compare this object to another
        /// </summary>
        /// <param name="obj">The other object to compare to</param>
        /// <returns>0, if equal, &lt;0 if less and &gt;0 if greater</returns>
        public int CompareTo(object obj)
        {
            return CompareTo((OptionalValue)obj);
        }

        /// <summary>
        /// Compare this object to another
        /// </summary>
        /// <param name="other">The other object to compare to</param>
        /// <returns>0, if equal, &lt;0 if less and &gt;0 if greater</returns>
        public int CompareTo(OptionalValue other)
        {
            return OptionalValueComparer.Default.Compare(this, other);
        }

        /// <summary>
        /// Determines whether this object equals to another of the same type.
        /// </summary>
        /// <param name="other">The object to compare to</param>
        /// <returns>true, when both objects are equal</returns>
        public bool Equals(OptionalValue other)
        {
            return OptionalValueComparer.Default.Equals(this, other);
        }

        /// <summary>
        /// Determines whether this object equals to another of the same type.
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>true, when both objects are equal</returns>
        public override bool Equals(object obj)
        {
            return Equals((OptionalValue)obj);
        }

        /// <summary>
        /// Returns the hash code.
        /// </summary>
        /// <returns>
        /// Ein Hashcode für das aktuelle Objekt.
        /// </returns>
        public override int GetHashCode() => OptionalValueComparer.Default.GetHashCode(this);

        /// <summary>
        /// Returns the status and value of the <see cref="OptionalValue"/>.
        /// </summary>
        /// <returns>the status and value of the <see cref="OptionalValue"/></returns>
        public override string ToString()
        {
            switch (_status)
            {
                case Status.Invalid:
                    return "-invalid-";
                case Status.Missing:
                    return "-missing-";
                case Status.Nil:
                    return "-nil-";
            }
            return _value ?? string.Empty;
        }

        internal int CompareStatus(OptionalValue other) => ((int)_status).CompareTo((int)other._status);

        internal int StatusHashCode => _status.GetHashCode();
    }
}
