using System;

namespace BeanIO
{
    /// <summary>
    /// The event arguments for the bean reader error
    /// </summary>
    public class BeanReaderErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeanReaderErrorEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception to pass to the event handler</param>
        public BeanReaderErrorEventArgs(BeanReaderException exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the exception that occurred while reading a bean.
        /// </summary>
        public BeanReaderException Exception { get; private set; }
    }
}
