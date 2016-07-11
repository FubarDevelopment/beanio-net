// <copyright file="AnnotatedFloor.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Annotation;

namespace BeanIO.Parser.Annotation
{
    [UnboundField("floor", At = 0, Literal = "hardwood")]
    public class AnnotatedFloor
    {
        [Field(At = 1)]
        public int Width { get; set; }

        [Field(At = 2)]
        public int Height { get; set; }
    }
}
