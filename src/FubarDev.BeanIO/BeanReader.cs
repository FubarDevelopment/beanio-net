// <copyright file="BeanReader.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO
{
    /// <summary>
    /// Abstract basic bean reader implementation.
    /// </summary>
    public abstract class BeanReader : IBeanReader
    {
        /// <summary>
        /// Error handler to handle exceptions thrown by <see cref="IBeanReader.Read"/>.
        /// </summary>
        public event BeanReaderErrorHandlerDelegate? Error;

        /// <summary>
        /// Gets or sets the record or group name of the most recent bean object read from this reader,
        /// or null if the end of the stream was reached.
        /// </summary>
        public string? RecordName { get; protected set; }

        /// <summary>
        /// Gets or sets the starting line number of the first record for the most recent bean
        /// object read from this reader, or -1 when the end of the stream is reached.
        /// The line number may be zero if new lines are not used to separate characters.
        /// </summary>
        public int LineNumber { get; protected set; }

        /// <summary>
        /// Gets the number of records read from the underlying input stream for the
        /// most recent bean object read from this reader.  This typically returns 1
        /// unless a bean object was mapped to a record group which may span
        /// multiple records.
        /// </summary>
        public abstract int RecordCount { get; }

        /// <summary>
        /// Gets the record information for all bean objects read from this reader.
        /// If a bean object can span multiple records, <see cref="IBeanReader.RecordCount"/> can be used
        /// to determine how many records were read from the stream.
        /// </summary>
        /// <param name="index">the index of the record, starting at 0.</param>
        /// <returns>the <see cref="IRecordContext"/>.</returns>
        public abstract IRecordContext GetRecordContext(int index);

        /// <summary>
        /// Reads a single bean from the input stream.
        /// </summary>
        /// <remarks>
        /// If the end of the stream is reached, null is returned.
        /// </remarks>
        /// <returns>The bean read, or null if the end of the stream was reached.</returns>
        public abstract object? Read();

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
        public abstract int Skip(int count);

        /// <summary>
        /// Closes the underlying input stream.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Disposes this resource.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Triggers the <see cref="Error" /> event or throws the exception when no event handler was given.
        /// </summary>
        /// <param name="exception">The exception to be passed to the event.</param>
        /// <returns>true when the error was passed to an event handler.</returns>
        protected virtual bool OnError(BeanReaderException exception)
        {
            var tmp = Error;

            if (tmp == null)
                return false;

            try
            {
                tmp(new BeanReaderErrorEventArgs(exception));
                return true;
            }
            catch (BeanReaderException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new BeanReaderException("Exception thrown by error handler", e);
            }
        }
    }
}
