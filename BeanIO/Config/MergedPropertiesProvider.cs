// <copyright file="MergedPropertiesProvider.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Linq;

namespace BeanIO.Config
{
    /// <summary>
    /// Merges properties from two <see cref="IPropertiesProvider"/> instances.
    /// </summary>
    public class MergedPropertiesProvider : IPropertiesProvider
    {
        private readonly IPropertiesProvider _basePropertiesReader;

        private readonly IPropertiesProvider _overridingPropertiesReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedPropertiesProvider"/> class.
        /// </summary>
        /// <param name="baseProperties">The base properties reader</param>
        /// <param name="overridingProperties">The <see cref="IPropertiesProvider"/> that overrides the values from <paramref name="baseProperties"/></param>
        public MergedPropertiesProvider(IPropertiesProvider baseProperties, IPropertiesProvider overridingProperties)
        {
            _basePropertiesReader = baseProperties;
            _overridingPropertiesReader = overridingProperties;
        }

        /// <summary>
        /// Reads all properties
        /// </summary>
        /// <returns>A dictionary with all properties read</returns>
        public Properties Read()
        {
            var result = _basePropertiesReader.Read()
                .ToDictionary(x => x.Key, x => x.Value);
            foreach (var keyValuePair in _overridingPropertiesReader.Read())
            {
                result[keyValuePair.Key] = keyValuePair.Value;
            }

            return new Properties(result);
        }
    }
}
