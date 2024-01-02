// <copyright file="DictionaryPropertiesProvider.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

namespace BeanIO.Config
{
    /// <summary>
    /// Provides properties using a given dictionary.
    /// </summary>
    public class DictionaryPropertiesProvider : IPropertiesProvider
    {
        private readonly IReadOnlyDictionary<string, string?> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryPropertiesProvider"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary to be returned by the <see cref="Read"/> function.</param>
        public DictionaryPropertiesProvider(IReadOnlyDictionary<string, string?> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// Reads all properties.
        /// </summary>
        /// <returns>A dictionary with all properties read.</returns>
        public Properties Read()
        {
            return new Properties(_dictionary);
        }
    }
}
