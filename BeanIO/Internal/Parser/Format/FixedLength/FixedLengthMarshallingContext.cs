using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser.Format.FixedLength
{
    /// <summary>
    /// A <see cref="MarshallingContext"/> for a fixed length formatted stream.
    /// </summary>
    internal class FixedLengthMarshallingContext : MarshallingContext
    {
        /// <summary>
        /// the list of entries for creating the record (may be unordered)
        /// </summary>
        private readonly List<Entry> _entries = new List<Entry>();

        /// <summary>
        /// the index of the last committed field in the record
        /// </summary>
        private int _committed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedLengthMarshallingContext"/> class.
        /// </summary>
        public FixedLengthMarshallingContext()
        {
            Filler = ' ';
        }

        /// <summary>
        /// Gets or sets the filler character for missing fields
        /// </summary>
        public char Filler { get; set; }

        /// <summary>
        /// Inserts field text into the record being marshalled
        /// </summary>
        /// <param name="position">the position of the field in the record</param>
        /// <param name="text">the field text to insert</param>
        /// <param name="commit">true to commit the current field length, or false
        /// if the field is optional and should not extend the record length
        /// unless a subsequent field is appended to the record</param>
        public void SetFieldText(int position, string text, bool commit)
        {
            var index = GetAdjustedFieldPosition(position);
            var entry = new Entry(index, text);
            _entries.Add(entry);
            if (commit)
                _committed = _entries.Count;
        }

        /// <summary>
        /// Clear is invoked after each bean object (record or group) is marshalled
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _committed = 0;
            _entries.Clear();
        }

        /// <summary>
        /// Creates the record object to pass to the <see cref="IRecordWriter"/>
        /// when <see cref="MarshallingContext.WriteRecord"/> is called.
        /// </summary>
        /// <returns>
        /// The newly created record object.
        /// </returns>
        protected override object ToRecordObject()
        {
            var record = new StringBuilder();
            var committedEntries = _entries.Take(_committed).OrderBy(x => x.Order);

            // the current index to write out
            int size = 0;

            // the offset for positions relative to the end of the record
            int offset = -1;

            foreach (var entry in committedEntries)
            {
                int index = entry.Position;
                if (index < 0)
                {
                    // the offset is calculated the first time we encounter
                    // a position relative to the end of the record
                    if (offset == -1)
                    {
                        offset = size + Math.Abs(index);
                        index = size;
                    }
                    else
                    {
                        index += offset;
                    }
                }

                if (index < size)
                {
                    record.Remove(index, entry.Text.Length);
                    record.Insert(index, entry.Text);
                    size = record.Length;
                }
                else
                {
                    if (index > size)
                    {
                        record.Append(Filler, index - size);
                        size = index;
                    }

                    record.Append(entry.Text);
                    size += entry.Text.Length;
                }
            }

            return record.ToString();
        }

        private class Entry
        {
            public Entry(int position, string text)
            {
                Position = position;
                Order = position < 0 ? position + int.MaxValue : position;
                Text = text;
            }

            public int Order { get; private set; }

            public string Text { get; private set; }

            public int Position { get; private set; }

            public override string ToString()
            {
                return string.Format("{0}:{1}", Order, Text);
            }
        }
    }
}
