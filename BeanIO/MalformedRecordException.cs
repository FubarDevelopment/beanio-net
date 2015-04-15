using System;
using System.IO;

using BeanIO;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown when a record does not adhere to the expected syntax of the stream format.
    /// </summary>
    /// <remarks>
    /// Subsequent calls to <see cref="IBeanReader.Read"/> are not likely to be successful.
    /// </remarks>
    public class MalformedRecordException : BeanReaderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MalformedRecordException" /> class.
        /// </summary>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public MalformedRecordException(params IRecordContext[] contexts)
            : base(contexts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MalformedRecordException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public MalformedRecordException(string message, params IRecordContext[] contexts)
            : base(message, contexts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MalformedRecordException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public MalformedRecordException(string message, Exception inner, params IRecordContext[] contexts)
            : base(message, inner, contexts)
        {
        }
    }
}
