using System;
using System.IO;

namespace BeanIO.Internal.Util
{
    public class MarkableTextReader : TextReader
    {
        private int? _peekBuffer;

        private int[] _markBuffer;

        private int _markBufferPosition;

        private int _markBufferSize;

        public MarkableTextReader(TextReader innerReader)
        {
            BaseReader = innerReader;
        }

        public TextReader BaseReader { get; private set; }

        private bool HasRemainingMarkBufferData
        {
            get
            {
                return _markBuffer != null && _markBufferPosition < _markBufferSize;
            }
        }

        public override int Peek()
        {
            if (HasRemainingMarkBufferData)
                return _markBuffer[_markBufferPosition];
            if (_peekBuffer == null)
                _peekBuffer = BaseReader.Peek();
            return _peekBuffer.Value;
        }

        public override int Read()
        {
            _peekBuffer = null;

            if (HasRemainingMarkBufferData)
                return _markBuffer[_markBufferPosition++];

            if (_markBuffer != null && _markBufferPosition == _markBuffer.Length)
                _markBuffer = null;

            var result = BaseReader.Read();
            if (result == -1)
                return result;

            if (_markBuffer != null && _markBufferSize < _markBuffer.Length)
            {
                _markBuffer[_markBufferSize++] = result;
                ++_markBufferPosition;
            }

            return result;
        }

        public virtual void Mark(int readAheadLimit)
        {
            var oldBuffer = _markBuffer;
            if (oldBuffer != null && _markBufferPosition != _markBufferSize)
            {
                var remainingSize = _markBufferSize - _markBufferPosition;
                _markBuffer = new int[Math.Max(readAheadLimit, remainingSize)];
                Array.Copy(oldBuffer, _markBufferPosition, _markBuffer, 0, remainingSize);
                _markBufferSize = remainingSize;
            }
            else
            {
                _markBuffer = new int[readAheadLimit];
                _markBufferSize = 0;
            }
            _markBufferPosition = 0;
        }

        public virtual void Reset()
        {
            if (_markBuffer == null)
                throw new InvalidOperationException("Reset without Mark");
            _markBufferPosition = 0;
        }
    }
}
