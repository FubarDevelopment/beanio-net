// <copyright file="IPropertiesProvider.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Config
{
    /// <summary>
    /// Property provider interface
    /// </summary>
    public interface IPropertiesProvider
    {
        /// <summary>
        /// Reads all properties
        /// </summary>
        /// <returns>A dictionary with all properties read</returns>
        Properties Read();
    }
}
