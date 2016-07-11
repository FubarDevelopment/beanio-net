// <copyright file="AnnotatedColor.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Annotation;

namespace BeanIO.Parser.Constructor
{
    [Record]
    public class AnnotatedColor
    {
        private readonly string _name;

        private readonly int _r;

        private readonly int _g;

        private readonly int _b;

        public AnnotatedColor()
            : this("black", 0, 0, 0)
        {
        }

        public AnnotatedColor(
            string name,
            [Field("r", At = 1)] int r,
            [Field("g", At = 2)] int g,
            [Field("b", At = 3)] int b)
        {
            _name = name;
            _r = r;
            _g = g;
            _b = b;
        }

        [Field(At = 0, Setter = "#1")]
        public string Name
        {
            get { return _name; }
        }

        public int R
        {
            get { return _r; }
        }

        public int G
        {
            get { return _g; }
        }

        public int B
        {
            get { return _b; }
        }
    }
}
