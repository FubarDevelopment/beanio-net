// <copyright file="IRecordParserFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.IO;

namespace BeanIO.Stream
{
    /// <summary>
    /// Factory interface for creating record parsers.
    /// </summary>
    /// <remarks>
    /// <p>The Java object bound to a <i>record</i> depends on the stream format.
    /// The following table shows the object used for each format:</p>
    /// <table summary="">
    /// <tr>
    ///   <th>Format</th>
    ///   <th>Record Type</th>
    /// </tr>
    ///  <tr>
    ///   <td>Fixed Length</td>
    ///   <td><c>String</c></td>
    /// </tr>
    /// <tr>
    ///   <td>CSV, Delimited</td>
    ///   <td><c>String[]</c></td>
    /// </tr>
    /// <tr>
    ///   <td>XML</td>
    ///   <td>{@link Document}</td>
    /// </tr>
    /// </table>
    /// <p>The following table shows the method invoked for a requested BeanIO interface.</p>
    /// <table summary="">
    /// <tr>
    ///   <th>Requests For</th>
    ///   <th>Invokes</th>
    /// </tr>
    /// <tr>
    ///   <td><see cref="IBeanReader"/></td>
    ///   <td><see cref="CreateReader(TextReader)"/></td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="IBeanWriter"/></td>
    ///   <td><see cref="CreateWriter(TextWriter)"/></td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="IUnmarshaller"/></td>
    ///   <td><see cref="CreateUnmarshaller()"/></td>
    /// </tr>
    /// <tr>
    ///   <td><see cref="IMarshaller"/></td>
    ///   <td><see cref="CreateMarshaller()"/></td>
    /// </tr>
    /// </table>
    /// <p>A <c>RecordParserFactory</c> implementation must be thread safe (after all of its properties have been set).</p>
    /// </remarks>
    public interface IRecordParserFactory
    {
        /// <summary>
        /// Initializes the factory.
        /// </summary>
        /// <remarks>
        /// This method is called when a mapping file is loaded after
        /// all parser properties have been set, and is therefore ideally used to preemptively
        /// validate parser configuration settings.
        /// </remarks>
        void Init();

        /// <summary>
        /// Creates a parser for reading records from an input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The created <see cref="IRecordReader"/>.</returns>
        IRecordReader CreateReader(TextReader reader);

        /// <summary>
        /// Creates a parser for writing records to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        /// <returns>The new <see cref="IRecordWriter"/>.</returns>
        IRecordWriter CreateWriter(TextWriter writer);

        /// <summary>
        /// Creates a parser for marshalling records.
        /// </summary>
        /// <returns>The created <see cref="IRecordMarshaller"/>.</returns>
        IRecordMarshaller CreateMarshaller();

        /// <summary>
        /// Creates a parser for unmarshalling records.
        /// </summary>
        /// <returns>The created <see cref="IRecordUnmarshaller"/>.</returns>
        IRecordUnmarshaller CreateUnmarshaller();
    }
}
