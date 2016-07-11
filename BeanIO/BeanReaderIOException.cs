// <copyright file="BeanReaderIOException.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown when a <see cref="IBeanReader"/>'s underlying input stream throws an <see cref="IOException"/>.
    /// </summary>
    public class BeanReaderIOException : BeanReaderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeanReaderIOException" /> class.
        /// </summary>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public BeanReaderIOException(params IRecordContext[] contexts)
            : base(contexts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanReaderIOException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public BeanReaderIOException(string message, params IRecordContext[] contexts)
            : base(message, contexts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanReaderIOException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public BeanReaderIOException(string message, IOException inner, params IRecordContext[] contexts)
            : base(message, inner, contexts)
        {
            Clause = inner;
        }

        /// <summary>
        /// Gets the IO exception or null.
        /// </summary>
        public IOException Clause { get; }
    }
}
