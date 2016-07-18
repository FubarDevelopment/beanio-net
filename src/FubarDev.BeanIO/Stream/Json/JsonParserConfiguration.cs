// <copyright file="JsonParserConfiguration.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Stream.Json
{
    /// <summary>
    /// Stores configuration settings for parsing JSON formatted streams
    /// </summary>
    public class JsonParserConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonParserConfiguration"/> class.
        /// </summary>
        public JsonParserConfiguration()
        {
            Indentation = 2;
        }

        /// <summary>
        /// Gets or sets a value indicating whether JSON output should be formatted prettily
        /// </summary>
        public bool Pretty { get; set; }

        /// <summary>
        /// Gets or sets the number of spaces to indent when <code>pretty</code> is enabled.
        /// </summary>
        /// <remarks>
        /// Defaults to 2
        /// </remarks>
        public int Indentation { get; set; }

        /// <summary>
        /// Gets or sets the line separator to use when <code>pretty</code> is enabled
        /// </summary>
        public string LineSeparator { get; set; }
    }
}
