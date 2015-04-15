using System;

namespace BeanIO
{
    /// <summary>
    /// Base class for all exceptions thrown by the BeanIO framework
    /// </summary>
    public class BeanIOException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeanIOException" /> class.
        /// </summary>
        public BeanIOException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanIOException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public BeanIOException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanIOException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        public BeanIOException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
