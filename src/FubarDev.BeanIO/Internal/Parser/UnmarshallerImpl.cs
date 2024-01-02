// <copyright file="UnmarshallerImpl.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Xml.Linq;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser
{
    internal class UnmarshallerImpl : IUnmarshaller
    {
        private readonly UnmarshallingContext _context;

        private readonly ISelector? _layout;

        private object? _recordValue;

        private string? _recordText;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmarshallerImpl"/> class.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/>.</param>
        /// <param name="layout">the stream layout.</param>
        /// <param name="recordUnmarshaller">the <see cref="IRecordUnmarshaller"/> for converting record text to record values.</param>
        public UnmarshallerImpl(UnmarshallingContext context, ISelector layout, IRecordUnmarshaller recordUnmarshaller)
        {
            _context = context;
            _layout = layout;
            _context.RecordReader = new UnmarshallerReader(this, recordUnmarshaller);
        }

        /// <summary>
        /// Gets the record or group name of the most recent unmarshalled bean object.
        /// </summary>
        public string? RecordName { get; private set; }

        /// <summary>
        /// Gets record information for the most recent unmarshalled bean object.
        /// </summary>
        public IRecordContext RecordContext => _context.GetRecordContext(0);

        /// <summary>
        /// Unmarshals a bean object from the given record text.
        /// </summary>
        /// <remarks>
        /// This method is supported by all stream formats.
        /// </remarks>
        /// <param name="record">The record text to unmarshal.</param>
        /// <returns>The unmarshalled bean object.</returns>
        public object? Unmarshal(string record)
        {
            RecordName = null;
            _recordText = record ?? throw new ArgumentNullException(nameof(record));

            return Unmarshal();
        }

        /// <summary>
        /// Unmarshals a bean object from the given list of fields.
        /// </summary>
        /// <remarks>
        /// This method is supported by CSV and delimited formatted streams only.
        /// </remarks>
        /// <param name="fields">The fields to unmarshal.</param>
        /// <returns>The unmarshalled bean object.</returns>
        public object? Unmarshal(IList<string?> fields)
        {
            if (fields == null)
                throw new ArgumentNullException(nameof(fields));

            RecordName = null;
            _recordValue = _context.ToRecordValue(fields);

            if (_recordValue == null)
                throw new BeanReaderException("unmarshal(List) not supported by stream format");

            return Unmarshal();
        }

        /// <summary>
        /// Unmarshals a bean object from the given array of fields.
        /// </summary>
        /// <remarks>
        /// This method is supported by CSV and delimited formatted streams only.
        /// </remarks>
        /// <param name="fields">The fields to unmarshal.</param>
        /// <returns>The unmarshalled bean object.</returns>
        public object? Unmarshal(string?[] fields)
        {
            if (fields == null)
                throw new ArgumentNullException(nameof(fields));

            RecordName = null;
            _recordValue = _context.ToRecordValue(fields);

            if (_recordValue == null)
                throw new BeanReaderException("unmarshal(string[]) not supported by stream format");

            return Unmarshal();
        }

        /// <summary>
        /// Unmarshals a bean object from the given element.
        /// </summary>
        /// <param name="node">The <see cref="XElement"/> to unmarshal.</param>
        /// <returns>The unmarshalled bean object.</returns>
        public object? Unmarshal(XContainer node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            RecordName = null;
            _recordValue = _context.ToRecordValue(node);

            if (_recordValue == null)
                throw new BeanReaderException("unmarshal(XElement) not supported by stream format");

            return Unmarshal();
        }

        /// <summary>
        /// Internal unmarshal method.
        /// </summary>
        /// <returns>the unmarshalled object.</returns>
        private object? Unmarshal()
        {
            _context.NextRecord();

            ISelector? parser;
            try
            {
                parser = _layout?.MatchNext(_context);
            }
            catch (UnexpectedRecordException)
            {
                // when thrown, 'parser' is null and the error is handled below
                parser = null;
            }

            if (parser == null)
            {
                parser = _layout?.MatchAny(_context);
                if (parser != null)
                    throw _context.RecordUnexpectedException(parser.Name);
                throw _context.RecordUnidentifiedException();
            }

            RecordName = parser.Name;
            try
            {
                if (parser.IsRecordGroup)
                {
                    _context.RecordSkipped();
                    throw new BeanReaderException("Record groups not supported by Unmarshallers");
                }

                // notify the unmarshalling context that we are about to unmarshal a new record
                _context.Prepare(parser.Name, false);

                // unmarshal the record
                try
                {
                    parser.Unmarshal(_context);
                }
                catch (AbortRecordUnmarshalligException)
                {
                    // Ignores the aborted unmarshalling
                }

                // this will throw an exception if an invalid record was unmarshalled
                _context.Validate();

                // return the unmarshalled bean object
                return parser.GetValue(_context);
            }
            finally
            {
                parser.ClearValue(_context);
            }
        }

        private class UnmarshallerReader : IRecordReader
        {
            private readonly UnmarshallerImpl _unmarshaller;

            private readonly IRecordUnmarshaller _recordUnmarshaller;

            public UnmarshallerReader(UnmarshallerImpl unmarshaller, IRecordUnmarshaller recordUnmarshaller)
            {
                _recordUnmarshaller = recordUnmarshaller;
                _unmarshaller = unmarshaller;
            }

            /// <summary>
            /// Gets a single record from this input stream.
            /// </summary>
            /// <remarks>The type of object returned depends on the format of the stream.</remarks>
            /// <returns>
            /// The record value, or null if the end of the stream was reached.
            /// .</returns>
            public int RecordLineNumber => 0;

            /// <summary>
            /// Gets the unparsed record text of the last record read.
            /// </summary>
            /// <returns>
            /// The unparsed text of the last record read
            /// .</returns>
            public string? RecordText => _unmarshaller._recordText;

            /// <summary>
            /// Reads a single record from this input stream.
            /// </summary>
            /// <returns>
            /// The type of object returned depends on the format of the stream.
            /// .</returns>
            /// <returns>The record value, or null if the end of the stream was reached.</returns>
            public object? Read()
            {
                try
                {
                    var value = _unmarshaller._recordValue;
                    if (RecordText != null)
                        value = _recordUnmarshaller.Unmarshal(RecordText);
                    return value;
                }
                finally
                {
                    _unmarshaller._recordText = null;
                    _unmarshaller._recordValue = null;
                }
            }

            /// <summary>
            /// Closes this input stream.
            /// </summary>
            public void Close()
            {
            }
        }
    }
}
