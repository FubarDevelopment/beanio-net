// <copyright file="IBeanConfig.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Config;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// Stores bean information.
    /// </summary>
    /// <remarks>
    /// A <code>Bean</code> object is used to instantiate configurable components
    /// such as a type handler, record reader factory or record writer factory.
    /// </remarks>
    public interface IBeanConfig
    {
        /// <summary>
        /// Gets or sets the fully qualified class name of the bean.
        /// </summary>
        string ClassName { get; set; }

        /// <summary>
        /// Gets or sets the bean properties.
        /// </summary>
        Properties Properties { get; set; }
    }
}
