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

        public RecordFilterReader(TextReader innerReader)
            : base(innerReader)
        {
            LineNumber = 1;
        }

        public int LineNumber { get; private set; }

        public int Position { get; private set; }

        public void RecordStarted(string text = null)
        {
            _record = new StringBuilder(text ?? string.Empty);
        }

        public string RecordCompleted()
        {
            if (_record == null)
                throw new InvalidOperationException("RecordStarted() not called");
            var text = _record.ToString();
            _record = null;
            return text;
        }

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

        public override void Mark(int readAheadLimit)
        {
            base.Mark(readAheadLimit);
            _mark = _record == null ? -1 : _record.Length;
        }

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
