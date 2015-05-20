namespace BeanIO.Stream
{
    /// <summary>
    /// A <see cref="IRecordReader"/> is used to divide an input stream into records.
    /// The Java class used to represent a record is implementation specific and
    /// dependent on the format of the input stream.
    /// </summary>
    public interface IRecordReader
    {
        /// <summary>
        /// Gets a single record from this input stream.
        /// </summary>
        /// <remarks>The type of object returned depends on the format of the stream.</remarks>
        /// <returns>
        /// The record value, or null if the end of the stream was reached.
        /// </returns>
        int RecordLineNumber { get; }

        /// <summary>
        /// Gets the unparsed record text of the last record read.
        /// </summary>
        /// <returns>
        /// The unparsed text of the last record read
        /// </returns>
        string RecordText { get; }

        /// <summary>
        /// Reads a single record from this input stream.
        /// </summary>
        /// <returns>
        /// The type of object returned depends on the format of the stream.
        /// </returns>
        /// <returns>The record value, or null if the end of the stream was reached.</returns>
        object Read();

        /// <summary>
        /// Closes this input stream.
        /// </summary>
        void Close();
    }
}