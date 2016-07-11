// <copyright file="DomUtil.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Xml.Linq;

using BeanIO.Internal.Parser.Format.Xml;
using BeanIO.Stream;

using JetBrains.Annotations;

namespace BeanIO.Internal.Util
{
    internal static class DomUtil
    {
        public static void SetAnnotation<T>([NotNull] this XObject obj, T annotation)
            where T : class
        {
            obj.RemoveAnnotations<T>();
            obj.AddAnnotation(annotation);
        }

        public static XName ToConvertedName(this XName name, ElementNameConversionMode conversionMode)
        {
            var localName = name.LocalName.ToConvertedName(conversionMode);
            return XName.Get(localName, name.NamespaceName);
        }
    }
}
