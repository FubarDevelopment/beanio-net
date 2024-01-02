// <copyright file="StreamConfig.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using BeanIO.Builder;
using BeanIO.Stream;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// A stream is the root (a.k.a top or parent) group of a stream mapping configuration.
    /// As such, it contains other attributes that apply to the entire stream.
    /// </summary>
    /// <remarks>
    /// By default, a stream can be used for both marshalling (write) and unmarshalling
    /// (read).  Calling {@link #setMode(String)} can restrict the use of the stream, but
    /// will relax some validations on the types of objects that can be read or written.
    /// </remarks>
    public class StreamConfig : GroupConfig
    {
        private readonly List<TypeHandlerConfig> _typeHandlerConfigs = new List<TypeHandlerConfig>();

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamConfig"/> class.
        /// </summary>
        public StreamConfig()
        {
            MinOccurs = 0;
            MaxOccurs = 1;
            Order = 1;
            NameConversionMode = ElementNameConversionMode.Unchanged;
        }

        /// <summary>
        /// Gets the component type.
        /// </summary>
        /// <returns>
        /// One of <see cref="F:ComponentType.Group"/>,
        /// <see cref="F:ComponentType.Record"/>, <see cref="F:ComponentType.Segment"/>
        /// <see cref="F:ComponentType.Field"/>, <see cref="F:ComponentType.Constant"/>,
        /// <see cref="F:ComponentType.Wrapper"/>, or <see cref="F:ComponentType.Stream"/>
        /// .</returns>
        public override ComponentType ComponentType => ComponentType.Stream;

        /// <summary>
        /// Gets or sets the format of this stream.
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// Gets or sets the allowed mode(s) of operation for this stream.
        /// </summary>
        public AccessMode? Mode { get; set; }

        /// <summary>
        /// Gets or sets the full class name of the resource bundle containing customized error messages for this stream.
        /// </summary>
        public string? ResourceBundle { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ElementNameConversionMode"/>.
        /// </summary>
        public ElementNameConversionMode NameConversionMode { get; set; }

        /// <summary>
        /// Gets a list of customized type handlers configured for this stream.
        /// </summary>
        public IReadOnlyList<TypeHandlerConfig> Handlers => _typeHandlerConfigs;

        /// <summary>
        /// Gets or sets the record parser factory configuration bean.
        /// </summary>
        public BeanConfig<IRecordParserFactory>? ParserFactory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether BeanIO should calculate and enforce strict record ordering
        /// (based on the order records are declared in the mapping file) and record length
        /// (based on configured field occurrences).
        /// </summary>
        public bool IsStrict { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore unidentified records.
        /// </summary>
        public bool IgnoreUnidentifiedRecords { get; set; }

        /// <summary>
        /// Adds a custom type handler to this stream.
        /// </summary>
        /// <param name="handler">the type handler to add.</param>
        public void AddHandler(TypeHandlerConfig handler)
        {
            _typeHandlerConfigs.Add(handler);
        }
    }
}
