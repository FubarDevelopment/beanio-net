using System;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser.Format.FixedLength
{
    /// <summary>
    /// The <see cref="UnmarshallingContext"/> implementation for a fixed length formatted stream.
    /// </summary>
    internal class FixedLengthUnmarshallingContext : UnmarshallingContext
    {
        private string _record;

        /// <summary>
        /// Gets the length of the record being unmarshalled
        /// </summary>
        public int RecordLength { get; private set; }

        /// <summary>
        /// Sets the value of the record returned from the <see cref="IRecordReader"/>
        /// </summary>
        /// <param name="value">the record value read by a <see cref="IRecordReader"/></param>
        public override void SetRecordValue(object value)
        {
            _record = (string)value;
            RecordLength = _record == null ? 0 : _record.Length;
        }

        public string GetFieldText(string name, int position, int length, int until)
        {
            var max = RecordLength + until;
            if (position < 0)
            {
                position = RecordLength + position;
                position = GetAdjustedFieldPosition(position);
                if (position < 0)
                    return null;
            }
            else
            {
                position = GetAdjustedFieldPosition(position);
                if (position >= max)
                    return null;
            }

            string text;
            if (length < 0)
            {
                text = _record.Substring(position, max - position);
            }
            else
            {
                text = _record.Substring(position, Math.Min(max, position + length) - position);
            }

            SetFieldText(name, text);

            return text;
        }
    }
}
