using System;
using System.IO;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// Mimics the mark/reset functionality of Java streams.
    /// </summary>
    public class MarkableTextReader : TextReader
    {
        private int? _peekBuffer;

        private int[] _markBuffer;

        private int _markBufferPosition;

        private int _markBufferSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkableTextReader"/> class.
        /// </summary>
        /// <param name="innerReader">The reader to read from</param>
        public MarkableTextReader(TextReader innerReader)
        {
            BaseReader = innerReader;
        }

        /// <summary>
        /// Gets the reader this reader reads from.
        /// </summary>
        public TextReader BaseReader { get; private set; }

        private bool HasRemainingMarkBufferData
        {
            get
            {
                return _markBuffer != null && _markBufferPosition < _markBufferSize;
            }
        }

        /// <summary>
        /// Peeks for the next character
        /// </summary>
        /// <returns>The character that would be read next</returns>
        public override int Peek()
        {
            if (HasRemainingMarkBufferData)
                return _markBuffer[_markBufferPosition];
            if (_peekBuffer == null)
                _peekBuffer = BaseReader.Peek();
            return _peekBuffer.Value;
        }

        /// <summary>
        /// Reads the next character
        /// </summary>
        /// <returns>The next character that was read from the <see cref="BaseReader"/>,
        /// or -1 if the end of the stream was reached.</returns>
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

        /// <summary>
        /// Marks the position and initializes a buffer to be read from after a <see cref="Reset"/>.
        /// </summary>
        /// <param name="readAheadLimit">The buffer size</param>
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

        /// <summary>
        /// Resets the read position
        /// </summary>
        public virtual void Reset()
        {
            if (_markBuffer == null)
                throw new InvalidOperationException("Reset without Mark");
            _markBufferPosition = 0;
        }
    }
}
