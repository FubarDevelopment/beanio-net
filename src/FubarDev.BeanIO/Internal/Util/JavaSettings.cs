// <copyright file="JavaSettings.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Config;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// <see cref="ISettings"/> is used to load and store BeanIO configuration settings.
    /// </summary>
    internal class JavaSettings : ISettings
    {
        internal const string DEFAULT_CONFIGURATION_PATH = "BeanIO.Internal.Config.beanio.properties";

        private readonly Properties _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaSettings"/> class.
        /// </summary>
        /// <param name="properties">The properties to use for this settings object.</param>
        public JavaSettings(Properties properties)
        {
            _properties = properties;
        }

        /// <summary>
        /// Returns a BeanIO configuration setting
        /// </summary>
        /// <param name="key">the name of the setting</param>
        /// <returns>the value of the setting, or null if the name is invalid</returns>
        public string this[string key]
        {
            get
            {
                string result;
                if (_properties.TryGetValue(key, out result))
                    return result;
                return null;
            }
        }
    }
}
