namespace BeanIO.Stream
{
    /// <summary>
    /// A <see cref="IRecordWriter"/> is used to write records to an output stream.
    /// The class used to represent a <i>record</i> is implementation specific and
    /// dependent on the format of the output stream.  For example, a delimited stream
    /// may use <code>String[]</code> objects to define records, while a fixed length based
    /// stream may simply use <code>String</code>.
    /// </summary>
    public interface IRecordWriter
    {
        /// <summary>
        /// Writes a record object to this output stream.
        /// </summary>
        /// <param name="record">Record the record object to write</param>
        void Write(object record);

        /// <summary>
        /// Flushes the output stream.
        /// </summary>
        void Flush();

        /// <summary>
        /// Closes the output stream.
        /// </summary>
        void Close();
    }
}
