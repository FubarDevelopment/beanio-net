using System.IO;

using BeanIO.Stream;
using BeanIO.Stream.FixedLength;

namespace BeanIO.Internal.Parser.Format.FixedLength
{
    public class FixedLengthStreamFormat : StreamFormatSupport
    {
        /// <summary>
        /// Creates a new unmarshalling context
        /// </summary>
        /// <returns>the new <see cref="UnmarshallingContext"/></returns>
        public override UnmarshallingContext CreateUnmarshallingContext()
        {
            return new FixedLengthUnmarshallingContext();
        }

        /// <summary>
        /// Creates a new marshalling context
        /// </summary>
        /// <param name="streaming">true if marshalling to a stream</param>
        /// <returns>the new <see cref="MarshallingContext"/></returns>
        public override MarshallingContext CreateMarshallingContext(bool streaming)
        {
            return new FixedLengthMarshallingContext();
        }

        /// <summary>
        /// Creates a new record reader
        /// </summary>
        /// <param name="reader">the <see cref="TextReader"/> to read records from</param>
        /// <returns>the new <see cref="IRecordReader"/></returns>
        public override IRecordReader CreateRecordReader(TextReader reader)
        {
            return new FixedLengthReader(reader);
        }

        /// <summary>
        /// Creates a new record writer
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write records to</param>
        /// <returns>the new <see cref="IRecordWriter"/></returns>
        public override IRecordWriter CreateRecordWriter(TextWriter writer)
        {
            return new FixedLengthWriter(writer);
        }
    }
}
