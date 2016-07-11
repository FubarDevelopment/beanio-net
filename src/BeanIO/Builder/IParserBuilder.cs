// <copyright file="IParserBuilder.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Internal.Config;
using BeanIO.Stream;

namespace BeanIO.Builder
{
    /// <summary>
    /// The basic parser builder interface
    /// </summary>
    public interface IParserBuilder
    {
        /// <summary>
        /// Builds the configuration about the record parser factory.
        /// </summary>
        /// <returns>The configuration for the record parser factory.</returns>
        BeanConfig<IRecordParserFactory> Build();
    }
}
