namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// This exception may be thrown by <see cref="IParser.Unmarshal"/> to
    /// abort record unmarshalling after a critical validation error has occurred.
    /// </summary>
    public class AbortRecordUnmarshalligException : BeanIOException
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
