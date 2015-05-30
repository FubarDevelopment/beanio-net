using System;
using System.IO;

namespace BeanIO.Stream.FixedLength
{
    /// <summary>
    /// A <see cref="FixedLengthWriter"/> is used to write records to fixed length flat file or output stream.
    /// </summary>
    /// <remarks>
    /// A fixed length record is represented using the <see cref="string"/> class. 
    /// </remarks>
    public class FixedLengthWriter : IRecordWriter
    {
        private readonly TextWriter _writer;

        private readonly string _recordTerminator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedLengthWriter"/> class.
        /// </summary>
        /// <param name="writer">the output stream to write to</param>
        public FixedLengthWriter(TextWriter writer)
            : this(writer, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedLengthWriter"/> class.
        /// </summary>
        /// <param name="writer">the output stream to write to</param>
        /// <param name="recordTerminator">the text used to terminate a record</param>
        public FixedLengthWriter(TextWriter writer, string recordTerminator)
        {
            _writer = writer;
            _recordTerminator = recordTerminator ?? Environment.NewLine;
        }

        /// <summary>
        /// Writes a record object to this output stream.
        /// </summary>
        /// <param name="record">Record the record object to write</param>
        public void Write(object record)
        {
            _writer.Write(record.ToString());
            _writer.Write(_recordTerminator);
        }

        /// <summary>
        /// Flushes the output stream.
        /// </summary>
        public void Flush()
        {
            _writer.Flush();
        }

        /// <summary>
        /// Closes the output stream.
        /// </summary>
        public void Close()
        {
            _writer.Dispose();
        }
    }
}
