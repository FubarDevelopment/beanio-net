using System;
using System.Collections.Generic;
using System.Linq;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser.Format.Delimited
{
    /// <summary>
    /// A <see cref="MarshallingContext"/> for delimited records.
    /// </summary>
    public class DelimitedMarshallingContext : MarshallingContext
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
        /// Puts the field text in the record.
        /// </summary>
        /// <param name="position">the position of the field in the record</param>
        /// <param name="fieldText">the field text</param>
        /// <param name="commit">true to commit the current record, or false
        /// if the field is optional and should not extend the record
        /// unless a subsequent field is later appended to the record</param>
        public void SetField(int position, string fieldText, bool commit)
        {
            var index = GetAdjustedFieldPosition(position);
            var entry = new Entry(index, fieldText);
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
            _entries.Clear();
            _committed = 0;
        }

        /// <summary>
        /// Converts a record object to a <see cref="string"/>[].
        /// </summary>
        /// <param name="record">the record object to convert</param>
        /// <returns>the <see cref="string"/> array result, or null if not supported</returns>
        public override string[] ToArray(object record)
        {
            return (string[])record;
        }

        /// <summary>
        /// Converts a record object to a <see cref="IList{T}"/> with T=<see cref="string"/>.
        /// </summary>
        /// <param name="record">the record object to convert</param>
        /// <returns>the <see cref="IList{T}"/> result, or null if not supported</returns>
        public override IList<string> ToList(object record)
        {
            return new List<string>((string[])record);
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
            var record = new List<string>();
            var committedEntries = _entries.Take(_committed).OrderBy(x => x.Order);

            // the current index to write out
            var size = 0;

            // the offset for positions relative to the end of the record
            var offset = -1;

            foreach (var entry in committedEntries)
            {
                var index = entry.Position;
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
                    record[index] = entry.Text;
                }
                else
                {
                    while (index > size)
                    {
                        record.Add(string.Empty);
                        ++size;
                    }

                    record.Add(entry.Text);
                    ++size;
                }
            }

            return record.ToArray();
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
