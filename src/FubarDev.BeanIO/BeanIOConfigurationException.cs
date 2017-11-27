// <copyright file="BeanIOConfigurationException.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown when an invalid BeanIO configuration file is loaded.
    /// </summary>
    public class BeanIOConfigurationException : BeanIOException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeanIOConfigurationException" /> class.
        /// </summary>
        public BeanIOConfigurationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanIOConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public BeanIOConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanIOConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        public BeanIOConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
