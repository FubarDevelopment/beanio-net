using System;
using System.IO;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown when a <see cref="IBeanReader"/>'s underlying input stream throws an <see cref="IOException"/>.
    /// </summary>
    public class BeanReaderIOException : BeanReaderException
    {
        private readonly IOException _ioException;

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
            _ioException = inner;
        }

        /// <summary>
        /// Gets the IO exception or null.
        /// </summary>
        public IOException Clause
        {
            get { return _ioException; }
        }
    }
}
