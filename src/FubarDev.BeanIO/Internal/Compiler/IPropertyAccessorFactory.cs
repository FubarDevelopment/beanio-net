// <copyright file="IPropertyAccessorFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Internal.Parser;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Compiler
{
    /// <summary>
    /// Factory interface for creating <see cref="IPropertyAccessor"/> implementations.
    /// </summary>
    internal interface IPropertyAccessorFactory
    {
        /// <summary>
        /// Creates a new <see cref="IPropertyAccessor"/>
        /// </summary>
        /// <param name="parent">the parent bean object type</param>
        /// <param name="property">the property to access</param>
        /// <param name="carg">the constructor argument index</param>
        /// <returns>the new <see cref="IPropertyAccessor"/></returns>
        IPropertyAccessor CreatePropertyAccessor(Type parent, PropertyDescriptor property, int? carg);
    }
}
