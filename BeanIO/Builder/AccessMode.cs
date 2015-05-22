namespace BeanIO.Builder
{
    /// <summary>
    /// The allowed mode(s) of operation for a stream.
    /// </summary>
    public enum AccessMode
    {
        /// <summary>
        /// Only reading is allowed
        /// </summary>
        Read = 1,

        /// <summary>
        /// Only writing is allowed
        /// </summary>
        Write = 2,

        /// <summary>
        /// Reading and writing is allowed
        /// </summary>
        ReadWrite = 3,
    }
}
