using System;
using System.IO;
using System.Text;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// A <see cref="TextReader"/> implementation for tracking the current line number, current
    /// position and record text.
    /// </summary>
    public class RecordFilterReader : MarkableTextReader
    {
        private StringBuilder _record;

        private bool _skipLf;

        private int _mark = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordFilterReader"/> class.
        /// </summary>
        /// <param name="innerReader">The reader to read from</param>
        public RecordFilterReader(TextReader innerReader)
            : base(innerReader)
        {
            LineNumber = 1;
        }

        /// <summary>
        /// Gets the current line number
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Gets the current line position
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// Called when a new record was started
        /// </summary>
        /// <param name="text">The text the record was started with</param>
        public void RecordStarted(string text = null)
        {
            _record = new StringBuilder(text ?? string.Empty);
        }

        /// <summary>
        /// Called when the current record was completed
        /// </summary>
        /// <returns>The text the record was made of</returns>
        public string RecordCompleted()
        {
            if (_record == null)
                throw new InvalidOperationException("RecordStarted() not called");
            var text = _record.ToString();
            _record = null;
            return text;
        }

        /// <summary>
        /// Reads the next character
        /// </summary>
        /// <returns>The next character that was read from the <see cref="MarkableTextReader.BaseReader"/>,
        /// or -1 if the end of the stream was reached.</returns>
        public override int Read()
        {
            var n = base.Read();
            if (n == -1)
                return n;

            var c = char.ConvertFromUtf32(n);
            if (_record != null)
                _record.Append(c);

            switch (c)
            {
                case "\n":
                    if (_skipLf)
                    {
                        _skipLf = false;
                    }
                    else
                    {
                        LineNumber += 1;
                        Position = 0;
                    }
                    break;
                case "\r":
                    _skipLf = true;
                    LineNumber += 1;
                    Position = 0;
                    break;

                default:
                    Position += 1;
                    break;
            }

            return n;
        }

        /// <summary>
        /// Marks the position and initializes a buffer to be read from after a <see cref="MarkableTextReader.Reset"/>.
        /// </summary>
        /// <param name="readAheadLimit">The buffer size</param>
        public override void Mark(int readAheadLimit)
        {
            base.Mark(readAheadLimit);
            _mark = _record == null ? -1 : _record.Length;
        }

        /// <summary>
        /// Resets the read position
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            if (_mark < 0)
            {
                _record = null;
            }
            else
            {
                _record.Length = _mark;
            }
        }
    }
}
