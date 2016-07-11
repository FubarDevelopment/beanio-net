// <copyright file="BeanReaderException.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown by a <see cref="IBeanReader"/> or <see cref="IUnmarshaller"/>.
    /// </summary>
    /// <remarks>
    /// In most cases, a subclass of this exception is thrown. In a few (but rare) fatal cases,
    /// this exception may be thrown directly.
    /// </remarks>
    public class BeanReaderException : BeanIOException
    {
        private readonly IRecordContext[] _recordContexts;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanReaderException" /> class.
        /// </summary>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public BeanReaderException(params IRecordContext[] contexts)
        {
            _recordContexts = contexts;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanReaderException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public BeanReaderException(string message, params IRecordContext[] contexts)
            : base(message)
        {
            _recordContexts = contexts;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanReaderException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public BeanReaderException(string message, Exception inner, params IRecordContext[] contexts)
            : base(message, inner)
        {
            _recordContexts = contexts;
        }

        /// <summary>
        /// Gets the record context that caused the error.
        /// </summary>
        /// <remarks>
        /// May be null if there is no context information associated with the exception.
        /// If there is more than one record context, this method returns the context of
        /// the first record and is equivalent to calling <seealso cref="RecordContexts" />[0].
        /// </remarks>
        public IRecordContext RecordContext
        {
            get
            {
                if (_recordContexts == null || _recordContexts.Length == 0)
                    return null;
                return _recordContexts[0];
            }
        }

        /// <summary>
        /// Gets the unmarshalled record contexts.
        /// </summary>
        public IReadOnlyList<IRecordContext> RecordContexts => _recordContexts;
    }
}
