// <copyright file="Stream.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using BeanIO.Builder;

namespace BeanIO.Internal.Parser
{
    internal class Stream
    {
        private ISet<IParserLocal> _locals = new HashSet<IParserLocal>();
        private ISelector? _layout;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stream"/> class.
        /// </summary>
        /// <param name="format">the <see cref="IStreamFormat"/>.</param>
        public Stream(IStreamFormat format)
        {
            Format = format ?? throw new ArgumentNullException(nameof(format));
            Mode = AccessMode.ReadWrite;
        }

        /// <summary>
        /// Gets or sets the <see cref="IStreamFormat"/>.
        /// </summary>
        public IStreamFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISelector"/>.
        /// </summary>
        public ISelector Layout
        {
            get => _layout ?? throw new InvalidOperationException("Layout not initialized");
            set => _layout = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="AccessMode"/>.
        /// </summary>
        public AccessMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IMessageFactory"/>.
        /// </summary>
        public required IMessageFactory MessageFactory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore unidentified records.
        /// </summary>
        public bool IgnoreUnidentifiedRecords { get; set; }

        /// <summary>
        /// Gets the name of this stream.
        /// </summary>
        public string Name => Format.Name;

        /// <summary>
        /// Initializes the <see cref="Stream"/>.
        /// </summary>
        public void Init()
        {
            _locals = new HashSet<IParserLocal>();
            var parser = (Component)Layout;
            parser.RegisterLocals(_locals);
        }

        /// <summary>
        /// Creates a new <see cref="IBeanReader"/> for reading from the given input stream.
        /// </summary>
        /// <param name="textReader">the input stream to read from.</param>
        /// <param name="culture">the locale to use for rendering error messages.</param>
        /// <returns>the new <see cref="IBeanReader"/>.</returns>
        public IBeanReader CreateBeanReader(TextReader textReader, CultureInfo culture)
        {
            if (textReader == null)
                throw new ArgumentNullException(nameof(textReader));

            var context = Format.CreateUnmarshallingContext(MessageFactory);
            InitContext(context);
            context.Culture = culture;
            context.RecordReader = Format.CreateRecordReader(textReader);

            var reader = new BeanReaderImpl(context, Layout)
                {
                    IgnoreUnidentifiedRecords = IgnoreUnidentifiedRecords
                };
            return reader;
        }

        /// <summary>
        /// Creates a new <see cref="IBeanWriter"/> for writing to the given output stream.
        /// </summary>
        /// <param name="textWriter">the output stream to write to.</param>
        /// <returns>the new <see cref="IBeanWriter"/>.</returns>
        public IBeanWriter CreateBeanWriter(TextWriter textWriter)
        {
            if (textWriter == null)
                throw new ArgumentNullException(nameof(textWriter));

            var context = Format.CreateMarshallingContext(true);
            InitContext(context);
            context.RecordWriter = Format.CreateRecordWriter(textWriter);

            var writer = new BeanWriterImpl(context, Layout);
            return writer;
        }

        /// <summary>
        /// Creates a new <see cref="IUnmarshaller"/>.
        /// </summary>
        /// <param name="culture">the locale to use for rendering error messages.</param>
        /// <returns>the new <see cref="IUnmarshaller"/>.</returns>
        public IUnmarshaller CreateUnmarshaller(CultureInfo culture)
        {
            var recordUnmarshaller = Format.CreateRecordUnmarshaller();
            if (recordUnmarshaller == null)
                throw new InvalidOperationException("Unmarshaller not supported for stream format");

            var context = Format.CreateUnmarshallingContext(MessageFactory);
            InitContext(context);
            context.Culture = culture;

            return new UnmarshallerImpl(context, Layout, recordUnmarshaller);
        }

        /// <summary>
        /// Creates a new <see cref="IMarshaller"/>.
        /// </summary>
        /// <returns>the new <see cref="IMarshaller"/>.</returns>
        public IMarshaller CreateMarshaller()
        {
            var recordMarshaller = Format.CreateRecordMarshaller();
            if (recordMarshaller == null)
                throw new InvalidOperationException("Marshaller not supported for stream format");

            var context = Format.CreateMarshallingContext(false);
            InitContext(context);

            return new MarshallerImpl(context, Layout, recordMarshaller);
        }

        private void InitContext(ParsingContext context)
        {
            context.CreateHeap(_locals.Count);
            var i = 0;
            foreach (var local in _locals)
                local.Init(i++, context);
        }
    }
}
