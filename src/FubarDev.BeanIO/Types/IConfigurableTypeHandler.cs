// <copyright file="IConfigurableTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Config;
using BeanIO.Internal.Util;

namespace BeanIO.Types
{
    /// <summary>
    /// Interface for type handlers that support field specific customization.
    /// </summary>
    /// <remarks>
    /// When a type handler is registered that implements this interface, the
    /// <see cref="TypeHandlerFactory" /> invokes <see cref="Configure"/>
    /// if any type handler field properties were set.
    /// </remarks>
    public interface IConfigurableTypeHandler : ITypeHandler
    {
        /// <summary>
        /// Configures this type handler.
        /// </summary>
        /// <param name="properties">The properties for customizing the instance</param>
        void Configure(Properties properties);
    }
}
