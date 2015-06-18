using System;
using System.IO;

using JetBrains.Annotations;

namespace BeanIO.Stream
{
    public class StrictStringReader : TextReader
    {
        private int _pos = 0;

        private char[] _c;

        public StrictStringReader([NotNull] string s)
        {
            _c = s.ToCharArray();
        }

        public override int Read()
        {
            if (_pos == -1)
                throw new ObjectDisposedException(typeof(StrictStringReader).Name);
            if (_pos < _c.Length)
                return _c[_pos++];
            _pos = -1;
            return -1;
        }

        public override void Close()
        {
        }
    }
}
