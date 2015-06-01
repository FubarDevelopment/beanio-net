using System;

namespace BeanIO
{
    public interface IBeanReader : IDisposable
    {
        /// <summary>
        /// Error handler to handle exceptions thrown by <see cref="Read"/>.
        /// </summary>
        event BeanReaderErrorHandlerDelegate Error;

        /// <summary>
        /// Gets the record or group name of the most recent bean object read from this reader,
        /// or null if the end of the stream was reached.
        /// </summary>
        string RecordName { get; }

        /// <summary>
        /// Gets the starting line number of the first record for the most recent bean
        /// object read from this reader, or -1 when the end of the stream is reached.
        /// The line number may be zero if new lines are not used to separate characters.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets the number of records read from the underlying input stream for the
        /// most recent bean object read from this reader.  This typically returns 1
        /// unless a bean object was mapped to a record group which may span
        /// multiple records.
        /// </summary>
        int RecordCount { get; }

        /// <summary>
        /// Gets the record information for all bean objects read from this reader.
        /// If a bean object can span multiple records, <see cref="RecordCount"/> can be used
        /// to determine how many records were read from the stream.
        /// </summary>
        /// <param name="index">the index of the record, starting at 0</param>
        /// <returns>the <see cref="IRecordContext"/></returns>
        IRecordContext GetRecordContext(int index);

        /// <summary>
        /// Reads a single bean from the input stream.
        /// </summary>
        /// <remarks>
        /// If the end of the stream is reached, null is returned.
        /// </remarks>
        /// <returns>The bean read, or null if the end of the stream was reached.</returns>
        object Read();

        /// <summary>
        /// Skips ahead in the input stream.
        /// </summary>
        /// <remarks>
        /// Record validation errors are ignored, but a malformed record, unidentified
        /// record, or record out of sequence, will cause an exception that halts stream
        /// reading.  Exceptions thrown by this method are not passed to the error handler.
        /// </remarks>
        /// <param name="count">The number of bean objects to skip over that would have been
        /// returned by calling <see cref="Read"/>.
        /// </param>
        /// <returns>the number of skipped bean objects, which may be less than <paramref name="count"/>
        /// if the end of the stream was reached
        /// </returns>
        int Skip(int count);

        /// <summary>
        /// Closes the underlying input stream.
        /// </summary>
        void Close();
    }
}
