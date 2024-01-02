// <copyright file="AnnotatedBulb.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Annotation;

namespace BeanIO.Parser.Annotation
{
    public class AnnotatedBulb
    {
        [Field(At = 0)]
        public int Watts { get; set; }

        [Field(At = 1)]
        public string? Style { get; set; }
    }
}
