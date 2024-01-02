// <copyright file="BeanReaderImpl.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace BeanIO.Internal.Parser
{
    internal class BeanReaderImpl : BeanReader
    {
        private UnmarshallingContext? _context;

        private ISelector? _layout;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanReaderImpl"/> class.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/>.</param>
        /// <param name="layout">the root component of the parser tree.</param>
        public BeanReaderImpl(UnmarshallingContext context, ISelector? layout)
        {
            _context = context;
            _layout = layout;
        }

        /// <summary>
        /// Gets the number of records read from the underlying input stream for the
        /// most recent bean object read from this reader.  This typically returns 1
        /// unless a bean object was mapped to a record group which may span
        /// multiple records.
        /// </summary>
        public override int RecordCount => _context?.RecordCount ?? 0;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore unidentified records.
        /// </summary>
        public bool IgnoreUnidentifiedRecords { get; set; }

        [MemberNotNullWhen(true, nameof(_context), nameof(_layout))]
        private bool IsOpen => _context != null;

        /// <summary>
        /// Gets the record information for all bean objects read from this reader.
        /// If a bean object can span multiple records, <see cref="IBeanReader.RecordCount"/> can be used
        /// to determine how many records were read from the stream.
        /// </summary>
        /// <param name="index">the index of the record, starting at 0.</param>
        /// <returns>the <see cref="IRecordContext"/>.</returns>
        public override IRecordContext GetRecordContext(int index)
        {
            if (_context == null)
                throw new InvalidOperationException();
            return _context.GetRecordContext(index);
        }

        /// <summary>
        /// Reads a single bean from the input stream.
        /// </summary>
        /// <remarks>
        /// If the end of the stream is reached, null is returned.
        /// </remarks>
        /// <returns>The bean read, or null if the end of the stream was reached.</returns>
        public override object? Read()
        {
            EnsureOpen(IsOpen);

            while (true)
            {
                if (_layout == null)
                    return null;
                try
                {
                    var bean = InternalRead();
                    if (bean != null)
                        return bean;
                    if (_context.IsEof)
                        return null;
                }
                catch (BeanReaderException ex)
                {
                    // if an exception is thrown when parsing a dependent record,
                    // there is little chance of recovery
                    if (!OnError(ex))
                        throw;
                }
                catch (BeanIOException ex)
                {
                    // wrap the generic exception in a BeanReaderException
                    var e = new BeanReaderException("Fatal BeanIOException caught", ex);
                    if (!OnError(e))
                        throw;
                }
            }
        }

        /// <summary>
        /// Skips ahead in the input stream.
        /// </summary>
        /// <remarks>
        /// Record validation errors are ignored, but a malformed record, unidentified
        /// record, or record out of sequence, will cause an exception that halts stream
        /// reading.  Exceptions thrown by this method are not passed to the error handler.
        /// </remarks>
        /// <param name="count">The number of bean objects to skip over that would have been
        /// returned by calling <see cref="IBeanReader.Read"/>.
        /// </param>
        /// <returns>the number of skipped bean objects, which may be less than <paramref name="count"/>
        /// if the end of the stream was reached
        /// .</returns>
        public override int Skip(int count)
        {
            EnsureOpen(IsOpen);

            if (_layout == null)
                return 0;

            var n = 0;
            while (n < count)
            {
                // find the next matching record node
                var node = NextRecord();

                // node is null when the end of the stream is reached
                if (node == null)
                    return n;

                node.Skip(_context);

                // if the bean definition does not have a property type configured, it would not
                // have been mapped to a bean object
                if (node.Property != null)
                    ++n;
            }

            return n;
        }

        /// <summary>
        /// Closes the underlying input stream.
        /// </summary>
        public override void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            try
            {
                _context.RecordReader?.Close();
            }
            catch (IOException ex)
            {
                throw new BeanReaderIOException("Failed to close record reader", ex);
            }
            finally
            {
                _context = null;
            }
        }

        private void EnsureOpen([DoesNotReturnIf(false)] bool isOpen)
        {
            if (!isOpen)
                throw new BeanReaderIOException("Stream closed");
        }

        private object? InternalRead()
        {
            EnsureOpen(IsOpen);

            ISelector? parser = null;
            try
            {
                // match the next record, parser may be null if EOF was reached
                parser = NextRecord();
                if (parser == null)
                    return null;

                // notify the unmarshalling context that we are about to unmarshal a new record
                _context.Prepare(parser.Name, parser.IsRecordGroup);

                // unmarshal the record
                try
                {
                    parser.Unmarshal(_context);
                }
                catch (AbortRecordUnmarshalligException)
                {
                    // Ignore abortion errors
                }

                // this will throw an exception if an invalid record was unmarshalled
                _context.Validate();

                // return the unmarshalled bean object
                return parser.GetValue(_context);
            }
            finally
            {
                parser?.ClearValue(_context);
            }
        }

        /// <summary>
        /// Reads the next record from the input stream and returns the matching record node.
        /// </summary>
        /// <returns>the next matching record node, or <see langword="null" /> if the end of the stream was reached.</returns>
        private ISelector? NextRecord()
        {
            EnsureOpen(IsOpen);

            ISelector? parser = null;

            // clear the current record name
            RecordName = null;

            do
            {
                // read the next record
                _context.NextRecord();

                // validate all record nodes are satisfied when the end of the file is reached
                if (_context.IsEof)
                {
                    try
                    {
                        // calling close will determine if all min occurs have been met
                        var unsatisfied = _layout?.Close(_context);
                        if (unsatisfied != null)
                        {
                            if (unsatisfied.IsRecordGroup)
                                throw _context.NewUnsatisfiedGroupException(unsatisfied.Name);
                            throw _context.NewUnsatisfiedRecordException(unsatisfied.Name);
                        }

                        return null;
                    }
                    finally
                    {
                        _layout = null;
                        LineNumber = -1;
                    }
                }

                // update the last line number read
                LineNumber = _context.LineNumber;

                try
                {
                    parser = _layout?.MatchNext(_context);
                }
                catch (UnexpectedRecordException)
                {
                    // when thrown, 'parser' is null and the error is handled below
                }

                if (parser != null || !IgnoreUnidentifiedRecords)
                    break;

                _context.RecordSkipped();
            }
            while (true);

            if (parser == null)
            {
                parser = _layout?.MatchAny(_context);

                if (parser != null)
                    throw _context.RecordUnexpectedException(parser.Name);

                throw _context.RecordUnidentifiedException();
            }

            RecordName = parser.Name;
            return parser;
        }
    }
}
