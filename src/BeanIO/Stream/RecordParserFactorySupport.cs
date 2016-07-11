// <copyright file="RecordParserFactorySupport.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

namespace BeanIO.Stream
{
    /// <summary>
    /// A base class for implementing a custom <see cref="IRecordParserFactory"/>
    /// </summary>
    public class RecordParserFactorySupport : IRecordParserFactory
    {
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
            throw new NotSupportedException("BeanReader not supported");
        }

        /// <summary>
        /// Creates a parser for writing records to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to</param>
        /// <returns>The new <see cref="IRecordWriter"/></returns>
        public IRecordWriter CreateWriter(TextWriter writer)
        {
            throw new NotSupportedException("BeanWriter not supported");
        }

        /// <summary>
        /// Creates a parser for marshalling records.
        /// </summary>
        /// <returns>The created <see cref="IRecordMarshaller"/></returns>
        public IRecordMarshaller CreateMarshaller()
        {
            throw new NotSupportedException("Marshaller not supported");
        }

        /// <summary>
        /// Creates a parser for unmarshalling records.
        /// </summary>
        /// <returns>The created <see cref="IRecordUnmarshaller"/></returns>
        public IRecordUnmarshaller CreateUnmarshaller()
        {
            throw new NotSupportedException("Unmarshaller not supported");
        }
    }
}
