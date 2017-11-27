// <copyright file="NamespaceModeAnnotation.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser.Format.Xml.Annotations
{
    internal class NamespaceModeAnnotation
    {
        public NamespaceModeAnnotation(NamespaceHandlingMode handlingMode)
        {
            HandlingMode = handlingMode;
        }

        public NamespaceHandlingMode HandlingMode { get; private set; }
    }
}
