// <copyright file="BeanWriterIOException.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown when a <see cref="IBeanWriter"/>'s underlying output stream throws an <see cref="IOException"/>.
    /// </summary>
    public class BeanWriterIOException : BeanWriterException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeanWriterIOException" /> class.
        /// </summary>
        public BeanWriterIOException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanWriterIOException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public BeanWriterIOException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanWriterIOException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        public BeanWriterIOException(string message, IOException inner)
            : base(message, inner)
        {
            Clause = inner;
        }

        /// <summary>
        /// Gets the IO exception or null.
        /// </summary>
        public IOException Clause { get; }
    }
}
