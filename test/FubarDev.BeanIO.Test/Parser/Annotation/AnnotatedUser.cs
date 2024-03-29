// <copyright file="AnnotatedUser.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using BeanIO.Annotation;
using BeanIO.Builder;

namespace BeanIO.Parser.Annotation
{
    [Record(XmlName = "user", XmlNamespace = "http://org.beanio.test")]
    [UnboundField("type", At = 0, IsRecordIdentifier = true, Literal = "USER", XmlType = XmlNodeType.Attribute)]
    public class AnnotatedUser : AnnotatedUserSupport, IAnnotatedUserInterface
    {
        [Field(At = 6, Type = typeof(int), Padding = '0', Align = Align.Right, Length = 4)]
        private object? _age;

        [Field(At = 5, Format = "yyyy-MM-dd")]
        public DateTime BirthDate { get; set; }

        [Field(At = 7, MinOccurs = 2)]
        public IList<char?>? Letters { get; set; }

        [Field(At = 9, Until = -1, Type = typeof(int), CollectionType = typeof(ArrayList), MinOccurs = 1, MaxOccurs = -1)]
        public IList? Numbers { get; set; }

        [Field(At = -1)]
        public string? End { get; set; }

        public string?[]? Hands { get; set; }

        public object? Age
        {
            get { return _age; }
            set { _age = value; }
        }
    }
}
