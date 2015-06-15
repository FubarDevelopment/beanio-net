using System.IO;

namespace BeanIO.Stream.FixedLength
{
    /// <summary>
    /// Default <see cref="IRecordParserFactory"/> for the fixed length stream format.
    /// </summary>
    public class FixedLengthRecordParserFactory : FixedLengthParserConfiguration, IRecordParserFactory
    {
        private static readonly FixedLengthRecordParser _parser = new FixedLengthRecordParser();

        /// <summary>
        /// Initializes the factory.
        /// </summary>
        /// <remarks>
        /// This method is called when a mapping file is loaded after
        /// all parser properties have been set, and is therefore ideally used to preemptively
        /// validate parser configuration settings.
        /// </remarks>
        public void Init()
        {
        }

        /// <summary>
        /// Creates a parser for reading records from an input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from</param>
        /// <returns>The created <see cref="IRecordReader"/></returns>
        public IRecordReader CreateReader(TextReader reader)
        {
            return new FixedLengthReader(reader, this);
        }

        /// <summary>
        /// Creates a parser for writing records to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to</param>
        /// <returns>The new <see cref="IRecordWriter"/></returns>
        public IRecordWriter CreateWriter(TextWriter writer)
        {
            return new FixedLengthWriter(writer, RecordTerminator);
        }

        /// <summary>
        /// Creates a parser for marshalling records.
        /// </summary>
        /// <returns>The created <see cref="IRecordMarshaller"/></returns>
        public IRecordMarshaller CreateMarshaller()
        {
            return _parser;
        }

        /// <summary>
        /// Creates a parser for unmarshalling records.
        /// </summary>
        /// <returns>The created <see cref="IRecordUnmarshaller"/></returns>
        public IRecordUnmarshaller CreateUnmarshaller()
        {
            return _parser;
        }
    }
}