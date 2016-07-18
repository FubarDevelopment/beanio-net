// <copyright file="BeanWriterImpl.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="BeanReader"/> implementation.
    /// </summary>
    internal class BeanWriterImpl : IBeanWriter, IStatefulWriter
    {
        private ISelector _layout;

        private MarshallingContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeanWriterImpl"/> class.
        /// </summary>
        /// <param name="context">the <see cref="MarshallingContext"/></param>
        /// <param name="layout">the root <see cref="ISelector"/> node in the parsing tree</param>
        public BeanWriterImpl(MarshallingContext context, ISelector layout)
        {
            _context = context;
            _layout = layout;
        }

        public void Dispose()
        {
            if (_context == null)
                return;
            Close();
        }

        /// <summary>
        /// Writes a bean object to this output stream.
        /// </summary>
        /// <param name="bean">The bean object to write</param>
        public void Write(object bean)
        {
            Write(null, bean);
        }

        /// <summary>
        /// Writes a bean object to this output stream.
        /// </summary>
        /// <param name="recordName">The record or group name bound to the bean object from the mapping file.</param>
        /// <param name="bean">The bean object to write</param>
        public void Write(string recordName, object bean)
        {
            EnsureOpen();

            if (recordName == null && bean == null)
                throw new BeanWriterException("Bean identification failed: a record name or bean object must be provided");

            try
            {
                // set the name of the component to be marshalled (may be null if we're just matching on bean)
                _context.ComponentName = recordName;

                // set the bean to be marshalled on the context
                _context.Bean = bean;

                // find the parser in the layout that defines the given bean
                var matched = _layout.MatchNext(_context);
                if (matched == null)
                {
                    if (recordName != null)
                    {
                        throw new BeanWriterException(
                            $"Bean identification failed: record name '{recordName}' not matched at the current position{(bean != null ? " for bean class '" + bean.GetType() + "'" : string.Empty)}");
                    }

                    throw new BeanWriterException(
                        $"Bean identification failed: no record or group mapping for bean class '{bean}' at the current position");
                }

                // marshal the bean object
                matched.Marshal(_context);
            }
            catch (IOException e)
            {
                throw new BeanWriterIOException(e.Message, e);
            }
            catch (BeanIOException e) when (!(e is BeanWriterException))
            {
                // wrap the generic exception in a BeanReaderException
                throw new BeanWriterException("Fatal BeanIOException", e);
            }
        }

        /// <summary>
        /// Flushes this output stream.
        /// </summary>
        public void Flush()
        {
            EnsureOpen();
            try
            {
                _context.RecordWriter.Flush();
            }
            catch (IOException e)
            {
                throw new BeanWriterIOException(e.Message, e);
            }
        }

        /// <summary>
        /// Closes this output stream.
        /// </summary>
        public void Close()
        {
            EnsureOpen();
            try
            {
                _context.RecordWriter.Close();
            }
            catch (IOException e)
            {
                throw new BeanWriterIOException(e.Message, e);
            }
            finally
            {
                _context = null;
                _layout = null;
            }
        }

        /// <summary>
        /// Updates a <see cref="IDictionary{TKey,TValue}"/> with the current state of the Writer to allow for
        /// restoration at a later time
        /// </summary>
        /// <param name="ns">a string to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> to update with the latest state</param>
        public void UpdateState(string ns, IDictionary<string, object> state)
        {
            _layout.UpdateState(_context, ns + ".m", state);
            var writer = _context.RecordWriter as IStatefulWriter;
            writer?.UpdateState(ns + ".w", state);
        }

        /// <summary>
        /// Restores a <see cref="IDictionary{TKey,TValue}"/> of previously stored state information
        /// </summary>
        /// <param name="ns">a string to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> containing the state to restore</param>
        public void RestoreState(string ns, IReadOnlyDictionary<string, object> state)
        {
            _layout.RestoreState(_context, ns + ".m", state);

            var writer = _context.RecordWriter as IStatefulWriter;
            writer?.RestoreState(ns + ".w", state);
        }

        private void EnsureOpen()
        {
            if (_context == null)
                throw new BeanWriterIOException("Stream closed");
        }
    }
}
