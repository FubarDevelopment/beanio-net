using System;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown by a <see cref="IBeanWriter"/> or <see cref="IMarshaller"/>.
    /// </summary>
    /// <remarks>
    /// In most cases, a subclass of this exception is thrown. In a few (but rare) fatal cases,
    /// this exception may be thrown directly.
    /// </remarks>
    public class BeanWriterException : BeanIOException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeanWriterException" /> class.
        /// </summary>
        public BeanWriterException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanWriterException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public BeanWriterException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanWriterException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        public BeanWriterException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
