using System;

namespace BeanIO.Internal.Parser
{
    public sealed class Value : IComparable<Value>, IEquatable<Value>
    {
        /// <summary>
        /// Constant indicating the field did not pass validation
        /// </summary>
        public static readonly Value Invalid = new Value("-invalid-");

        /// <summary>
        /// Constant indicating the field was not present in the stream
        /// </summary>
        public static readonly Value Missing = new Value("-missing-");

        /// <summary>
        /// Constant indicating the field was nil (XML only)
        /// </summary>
        public static readonly Value Nil = new Value("-nil-");

        private readonly string _id;

        private Value(string id)
        {
            _id = id;
        }

        int IComparable<Value>.CompareTo(Value other)
        {
            return string.CompareOrdinal(_id, other._id);
        }

        bool IEquatable<Value>.Equals(Value other)
        {
            return string.Equals(_id, other._id, StringComparison.Ordinal);
        }
    }
}
