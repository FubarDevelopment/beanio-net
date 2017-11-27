// <copyright file="UnexpectedRecordException.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown when the record type of last record read by a <see cref="IBeanReader"/>
    /// is out of order based on the expected order defined by the stream's mapping file.
    /// </summary>
    /// <remarks>
    /// After this exception is thrown, further reads from the stream will likely result in
    /// further exceptions.
    /// </remarks>
    public class UnexpectedRecordException : BeanReaderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedRecordException" /> class.
        /// </summary>
        /// <param name="context">The record context that caused the exception</param>
        public UnexpectedRecordException(IRecordContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedRecordException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="context">The record context that caused the exception</param>
        public UnexpectedRecordException(string message, IRecordContext context)
            : base(message, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedRecordException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        /// <param name="context">The record context that caused the exception</param>
        public UnexpectedRecordException(string message, Exception inner, IRecordContext context)
            : base(message, inner, context)
        {
        }
    }
}
