// <copyright file="AbortRecordUnmarshalligException.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// This exception may be thrown by <see cref="IParser.Unmarshal"/> to
    /// abort record unmarshalling after a critical validation error has occurred.
    /// </summary>
    internal class AbortRecordUnmarshalligException : BeanIOException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbortRecordUnmarshalligException"/> class.
        /// </summary>
        public AbortRecordUnmarshalligException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbortRecordUnmarshalligException"/> class.
        /// </summary>
        /// <param name="message">the error message (for debugging purposes only)</param>
        public AbortRecordUnmarshalligException(string message)
            : base(message)
        {
        }
    }
}
