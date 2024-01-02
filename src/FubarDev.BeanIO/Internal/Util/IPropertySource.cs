// <copyright file="IPropertySource.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// A source of property values.
    /// </summary>
    internal interface IPropertySource
    {
        /// <summary>
        /// Returns the property value for a given key.
        /// </summary>
        /// <param name="key">the property key.</param>
        /// <returns>the property value.</returns>
        string? GetProperty(string key);
    }
}
