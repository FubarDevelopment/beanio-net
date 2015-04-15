using System;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown when the record type of the last record read from a <see cref="IBeanReader"/> could not be determined.
    /// </summary>
    /// <remarks>
    /// If the mapping file is used to enforce strict record ordering, further reads from the stream will likely cause further exceptions.
    /// </remarks>
    public class UnidentifiedRecordException : BeanReaderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedRecordException" /> class.
        /// </summary>
        /// <param name="context">The record context that caused the exception</param>
        public UnidentifiedRecordException(IRecordContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedRecordException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="context">The record context that caused the exception</param>
        public UnidentifiedRecordException(string message, IRecordContext context)
            : base(message, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnidentifiedRecordException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        /// <param name="context">The record context that caused the exception</param>
        public UnidentifiedRecordException(string message, Exception inner, IRecordContext context)
            : base(message, inner, context)
        {
        }
    }
}
