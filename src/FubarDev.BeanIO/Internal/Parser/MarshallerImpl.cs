// <copyright file="MarshallerImpl.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser
{
    internal class MarshallerImpl : IMarshaller
    {
        private readonly ISelector _layout;

        private readonly MarshallingContext _context;

        private readonly IRecordMarshaller _recordMarshaller;

        private object _recordValue;

        public MarshallerImpl(MarshallingContext context, ISelector layout, IRecordMarshaller recordMarshaller)
        {
            _context = context;
            _layout = layout;
            _recordMarshaller = recordMarshaller;

            _context.RecordWriter = new MarshallerRecordWriter(this);
        }

        /// <summary>
        /// Gets the current record value
        /// </summary>
        protected virtual object RecordValue => _recordValue;

        /// <summary>
        /// Marshals a single bean object.
        /// </summary>
        /// <param name="bean">The bean object to marshal</param>
        /// <returns>This <see cref="IMarshaller"/></returns>
        public IMarshaller Marshal(object bean)
        {
            return Marshal(null, bean);
        }

        /// <summary>
        /// Marshals a single bean object.
        /// </summary>
        /// <param name="recordName">The name of the record to marshal</param>
        /// <param name="bean">The bean object to marshal</param>
        /// <returns>This <see cref="IMarshaller"/></returns>
        public IMarshaller Marshal(string recordName, object bean)
        {
            _recordValue = null;

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
                            "Bean identification failed: " +
                            $"record name '{recordName}' not matched at the current position{(bean != null ? " for bean class '" + bean.GetType() + "'" : string.Empty)}");
                    }

                    throw new BeanWriterException(
                        $"Bean identification failed: no record or group mapping for bean class '{bean.GetType()}' at the current position");
                }

                if (matched.IsRecordGroup)
                {
                    throw new BeanWriterException("Record groups not supported by Marshaller");
                }

                // marshal the bean object
                matched.Marshal(_context);

                return this;
            }
            catch (IOException e)
            {
                // not actually possible since we've overridden the IRecordWriter
                throw new BeanWriterException(e.Message, e);
            }
            catch (BeanWriterException)
            {
                throw;
            }
            catch (BeanIOException e)
            {
                // wrap generic exceptions in a BeanReaderException
                throw new BeanWriterException(e.Message, e);
            }
            finally
            {
                _context.Clear();
            }
        }

        /// <summary>
        /// Returns the most recent marshalled bean object as an array for CSV
        /// and delimited formatted streams.
        /// </summary>
        /// <returns>The <see cref="string"/> array of fields</returns>
        public string[] AsArray()
        {
            var array = _context.ToArray(_recordValue);
            if (array == null)
                throw new BeanWriterException("toArray() not supported by stream format");
            return array;
        }

        /// <summary>
        /// Returns the most recent marshalled bean object as an <see cref="IList{T}"/> for CSV
        /// and delimited formatted streams.
        /// </summary>
        /// <returns>The <see cref="string"/> list of fields</returns>
        public IList<string> AsList()
        {
            var list = _context.ToList(_recordValue);
            if (list == null)
                throw new BeanWriterException("toList() not supported by stream format");
            return list;
        }

        /// <summary>
        /// Returns the most recent marshalled bean object as an <see cref="XDocument"/> for XML
        /// formatted streams.
        /// </summary>
        /// <returns>The <see cref="XDocument"/></returns>
        public XDocument AsDocument()
        {
            var document = _context.ToXDocument(_recordValue);
            if (document == null)
                throw new BeanWriterException("toNode() not supported by stream format");
            return document;
        }

        public override string ToString()
        {
            return _recordValue == null ? null : _recordMarshaller.Marshal(_recordValue);
        }

        private class MarshallerRecordWriter : IRecordWriter
        {
            private readonly MarshallerImpl _marshaller;

            public MarshallerRecordWriter(MarshallerImpl marshaller)
            {
                _marshaller = marshaller;
            }

            /// <summary>
            /// Writes a record object to this output stream.
            /// </summary>
            /// <param name="record">Record the record object to write</param>
            public void Write(object record)
            {
                _marshaller._recordValue = record;
            }

            /// <summary>
            /// Flushes the output stream.
            /// </summary>
            public void Flush()
            {
            }

            /// <summary>
            /// Closes the output stream.
            /// </summary>
            public void Close()
            {
            }
        }
    }
}
