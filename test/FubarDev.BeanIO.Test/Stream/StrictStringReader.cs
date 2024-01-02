// <copyright file="StrictStringReader.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

namespace BeanIO.Stream
{
    public class StrictStringReader : TextReader
    {
        private readonly char[] _c;

        private int _pos;

        public StrictStringReader(string s)
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
    }
}
