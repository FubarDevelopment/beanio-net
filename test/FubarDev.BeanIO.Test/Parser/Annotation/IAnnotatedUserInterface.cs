// <copyright file="IAnnotatedUserInterface.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Annotation;

namespace BeanIO.Parser.Annotation
{
    public interface IAnnotatedUserInterface
    {
        [Field(At = 3, MinOccurs = 2, RegEx = "((left)|(right))")]
        string[] Hands { get; }
    }
}
