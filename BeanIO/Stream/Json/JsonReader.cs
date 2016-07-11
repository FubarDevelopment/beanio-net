// <copyright file="JsonReader.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

using BeanIO.Internal.Util;

using Newtonsoft.Json.Linq;

using NJ = Newtonsoft.Json;

namespace BeanIO.Stream.Json
{
    /// <summary>
    /// A <see cref="IRecordReader"/> implementation for JSON formatted records.
    /// </summary>
    public class JsonReader : IRecordReader
    {
        private readonly RecordFilterReader _filter;

        private NJ.JsonTextReader _reader;

        private bool _isEof;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonReader"/> class.
        /// </summary>
        /// <param name="reader">the <see cref="TextReader"/> to read from</param>
        public JsonReader(TextReader reader)
        {
            _filter = (reader as RecordFilterReader) ?? new RecordFilterReader(reader);
            _reader = new NJ.JsonTextReader(_filter)
                {
                    SupportMultipleContent = true,
                    CloseInput = true,
                };
        }

        /// <summary>
        /// Gets a single record from this input stream.
        /// </summary>
        /// <remarks>The type of object returned depends on the format of the stream.</remarks>
        /// <returns>
        /// The record value, or null if the end of the stream was reached.
        /// </returns>
        public int RecordLineNumber { get; private set; }

        /// <summary>
        /// Gets the unparsed record text of the last record read.
        /// </summary>
        /// <returns>
        /// The unparsed text of the last record read
        /// </returns>
        public string RecordText { get; private set; }

        /// <summary>
        /// Reads a single record from this input stream.
        /// </summary>
        /// <returns>
        /// The type of object returned depends on the format of the stream.
        /// </returns>
        /// <returns>The record value, or null if the end of the stream was reached.</returns>
        public object Read()
        {
            if (_isEof)
                return null;

            try
            {
                while (_reader.Read())
                {
                    if (_reader.TokenType == NJ.JsonToken.StartObject)
                    {
                        RecordLineNumber = _reader.LineNumber;
                        _filter.RecordStarted("{");
                        var value = JToken.Load(_reader);
                        RecordText = _filter.RecordCompleted();
                        return value;
                    }

                    throw new RecordIOException(string.Format("Unexpected token {0}", _reader.TokenType));
                }
            }
            catch (NJ.JsonReaderException ex)
            {
                throw new RecordIOException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new RecordIOException(string.Format("{0} at line {1}, near position {2}", ex.Message, _reader.LineNumber, _reader.LinePosition), ex);
            }

            _isEof = true;
            return null;
        }

        /// <summary>
        /// Closes this input stream.
        /// </summary>
        public void Close()
        {
            if (_reader != null)
            {
                try
                {
                    _reader.Close();
                }
                finally
                {
                    _reader = null;
                }
            }
        }
    }
}
