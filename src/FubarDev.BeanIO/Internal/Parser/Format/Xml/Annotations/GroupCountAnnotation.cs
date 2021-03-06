// <copyright file="GroupCountAnnotation.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser.Format.Xml.Annotations
{
    internal class GroupCountAnnotation
    {
        public GroupCountAnnotation(int count)
        {
            Count = count;
        }

        public int Count { get; set; }
    }
}
