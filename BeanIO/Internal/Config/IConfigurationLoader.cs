// <copyright file="IConfigurationLoader.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using BeanIO.Config;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// A <see cref="IConfigurationLoader"/> is used to load BeanIO mapping configurations from
    /// an input stream.
    /// </summary>
    internal interface IConfigurationLoader
    {
        /// <summary>
        /// Loads a BeanIO configuration from an input stream
        /// </summary>
        /// <param name="input">the input stream to read the configuration from</param>
        /// <param name="properties">the <see cref="Properties"/> for expansion in the mapping file</param>
        /// <returns>a collection of loaded BeanIO configurations</returns>
        ICollection<BeanIOConfig> LoadConfiguration(System.IO.Stream input, Properties properties);
    }
}
