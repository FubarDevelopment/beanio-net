// <copyright file="DefaultConfigurationFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;

using BeanIO.Internal.Util;

namespace BeanIO.Config
{
    /// <summary>
    /// Factory that creates the default configuration stuff
    /// </summary>
    public static class DefaultConfigurationFactory
    {
        /// <summary>
        /// Returns the default <see cref="ISettings"/> implementation
        /// </summary>
        /// <returns>A new instance of the default <see cref="ISettings"/> implementation</returns>
        public static ISettings CreateDefaultSettings()
        {
            var defaultProvider = new PropertiesStreamProvider(typeof(JavaSettings).GetTypeInfo().Assembly.GetManifestResourceStream(JavaSettings.DEFAULT_CONFIGURATION_PATH));
            return new JavaSettings(defaultProvider.Read());
        }

        /// <summary>
        /// Returns the default <see cref="ISchemeProvider"/> implementation
        /// </summary>
        /// <returns>the default <see cref="ISchemeProvider"/> implementation</returns>
        public static ISchemeProvider CreateDefaultSchemeProvider()
        {
            return new DefaultSchemeProvider();
        }
    }
}
