// <copyright file="StreamFormatSupport.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.IO;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser
{
    internal abstract class StreamFormatSupport : IStreamFormat
    {
        /// <summary>
        /// Gets or sets the <see cref="IRecordParserFactory"/> used by this stream.
        /// </summary>
        public abstract IRecordParserFactory RecordParserFactory { get; set; }

        /// <summary>
        /// Gets or sets the name of the stream.
        /// </summary>
        public required string Name { get; set; }

        /// <inheritdoc />
        public abstract UnmarshallingContext CreateUnmarshallingContext(IMessageFactory messageFactory);

        /// <summary>
        /// Creates a new marshalling context.
        /// </summary>
        /// <param name="streaming">true if marshalling to a stream.</param>
        /// <returns>the new <see cref="MarshallingContext"/>.</returns>
        public abstract MarshallingContext CreateMarshallingContext(bool streaming);

        /// <summary>
        /// Creates a new record marshaller.
        /// </summary>
        /// <returns>the new <see cref="IRecordMarshaller"/>.</returns>
        public virtual IRecordMarshaller CreateRecordMarshaller()
        {
            return RecordParserFactory.CreateMarshaller();
        }

        /// <summary>
        /// Creates a new record unmarshaller.
        /// </summary>
        /// <returns>the new <see cref="IRecordUnmarshaller"/>.</returns>
        public virtual IRecordUnmarshaller CreateRecordUnmarshaller()
        {
            return RecordParserFactory.CreateUnmarshaller();
        }

        /// <summary>
        /// Creates a new record reader.
        /// </summary>
        /// <param name="reader">the <see cref="TextReader"/> to read records from.</param>
        /// <returns>the new <see cref="IRecordReader"/>.</returns>
        public virtual IRecordReader CreateRecordReader(TextReader reader)
        {
            return RecordParserFactory.CreateReader(reader);
        }

        /// <summary>
        /// Creates a new record writer.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write records to.</param>
        /// <returns>the new <see cref="IRecordWriter"/>.</returns>
        public virtual IRecordWriter CreateRecordWriter(TextWriter writer)
        {
            return RecordParserFactory.CreateWriter(writer);
        }
    }
}
