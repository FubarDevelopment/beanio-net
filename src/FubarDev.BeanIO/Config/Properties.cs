// <copyright file="Properties.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections;
using System.Collections.Generic;

namespace BeanIO.Config
{
    /// <summary>
    /// Replacement for Java Properties
    /// </summary>
    public class Properties : IReadOnlyDictionary<string, string>
    {
        private readonly IReadOnlyDictionary<string, string> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="Properties"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary to use to provide the properties</param>
        public Properties(IReadOnlyDictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// Gets the number of entries
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets all keys
        /// </summary>
        public IEnumerable<string> Keys => _dictionary.Keys;

        /// <summary>
        /// Gets all values
        /// </summary>
        public IEnumerable<string> Values => _dictionary.Values;

        /// <summary>
        /// Gets the property for the given key
        /// </summary>
        /// <param name="key">The key to query</param>
        /// <returns>the value of the key or null if not found</returns>
        public string this[string key]
        {
            get
            {
                string result;
                if (!_dictionary.TryGetValue(key, out result))
                    result = null;
                return result;
            }
        }

        /// <summary>
        /// Returns the enumerator for the properties
        /// </summary>
        /// <returns>the enumerator for the properties</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Is there a property with the given key?
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <returns>true when the key exists</returns>
        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Try to get a property using the given key
        /// </summary>
        /// <param name="key">the key to search for</param>
        /// <param name="value">the value to set when the key was found</param>
        /// <returns>true when the key was found</returns>
        public bool TryGetValue(string key, out string value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}
