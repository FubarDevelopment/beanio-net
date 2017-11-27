// <copyright file="RecordIOException.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Stream
{
    /// <summary>
    /// A <see cref="RecordIOException"/> is thrown when a <see cref="IRecordReader"/>
    /// or <see cref="IRecordUnmarshaller"/> encounters a malformed record.  Subsequent
    /// reads from a <see cref="IRecordReader"/> may or may not be possible.
    /// </summary>
    public class RecordIOException : BeanIOException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordIOException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public RecordIOException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordIOException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public RecordIOException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
