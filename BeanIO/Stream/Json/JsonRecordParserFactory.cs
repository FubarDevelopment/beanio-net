// <copyright file="JsonRecordParserFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

using Newtonsoft.Json.Linq;

namespace BeanIO.Stream.Json
{
    /// <summary>
    /// A <see cref="IRecordParserFactory"/> for JSON formatted streams.
    /// </summary>
    /// <remarks>
    /// Record objects are stored using a <see cref="JObject"/>.
    /// </remarks>
    public class JsonRecordParserFactory : JsonParserConfiguration, IRecordParserFactory
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
            return new JsonReader(reader);
        }

        /// <summary>
        /// Creates a parser for writing records to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to</param>
        /// <returns>The new <see cref="IRecordWriter"/></returns>
        public IRecordWriter CreateWriter(TextWriter writer)
        {
            return new JsonWriter(writer, this);
        }

        /// <summary>
        /// Creates a parser for marshalling records.
        /// </summary>
        /// <returns>The created <see cref="IRecordMarshaller"/></returns>
        public IRecordMarshaller CreateMarshaller()
        {
            return new JsonRecordMarshaller(this);
        }

        /// <summary>
        /// Creates a parser for unmarshalling records.
        /// </summary>
        /// <returns>The created <see cref="IRecordUnmarshaller"/></returns>
        public IRecordUnmarshaller CreateUnmarshaller()
        {
            return new JsonRecordUnmarshaller();
        }
    }
}
