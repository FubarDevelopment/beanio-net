// <copyright file="IStreamFormat.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.IO;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="IStreamFormat"/> provides format specific extensions for a <see cref="BeanIO.Stream"/> parser.
    /// </summary>
    internal interface IStreamFormat
    {
        /// <summary>
        /// Gets the name of the stream.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates a new unmarshalling context.
        /// </summary>
        /// <param name="messageFactory">The message factory.</param>
        /// <returns>the new <see cref="UnmarshallingContext"/>.</returns>
        UnmarshallingContext CreateUnmarshallingContext(IMessageFactory messageFactory);

        /// <summary>
        /// Creates a new marshalling context.
        /// </summary>
        /// <param name="streaming">true if marshalling to a stream.</param>
        /// <returns>the new <see cref="MarshallingContext"/>.</returns>
        MarshallingContext CreateMarshallingContext(bool streaming);

        /// <summary>
        /// Creates a new record marshaller.
        /// </summary>
        /// <returns>the new <see cref="IRecordMarshaller"/>.</returns>
        IRecordMarshaller CreateRecordMarshaller();

        /// <summary>
        /// Creates a new record unmarshaller.
        /// </summary>
        /// <returns>the new <see cref="IRecordUnmarshaller"/>.</returns>
        IRecordUnmarshaller CreateRecordUnmarshaller();

        /// <summary>
        /// Creates a new record reader.
        /// </summary>
        /// <param name="reader">the <see cref="TextReader"/> to read records from.</param>
        /// <returns>the new <see cref="IRecordReader"/>.</returns>
        IRecordReader CreateRecordReader(TextReader reader);

        /// <summary>
        /// Creates a new record writer.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write records to.</param>
        /// <returns>the new <see cref="IRecordWriter"/>.</returns>
        IRecordWriter CreateRecordWriter(TextWriter writer);
    }
}
