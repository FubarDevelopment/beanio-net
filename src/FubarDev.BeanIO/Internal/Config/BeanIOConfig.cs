// <copyright file="BeanIOConfig.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// Stores BeanIO stream mapping configuration settings.
    /// </summary>
    internal class BeanIOConfig
    {
        private readonly List<TypeHandlerConfig> _handlerList = new List<TypeHandlerConfig>();

        private readonly List<StreamConfig> _streamList = new List<StreamConfig>();

        /// <summary>
        /// Gets or sets the source of this configuration.
        /// </summary>
        /// <remarks>
        /// May be <see langword="null" /> if unknown or not specified.
        /// </remarks>
        public string? Source { get; set; }

        /// <summary>
        /// Gets the list of stream mappings for this configuration.
        /// </summary>
        public IReadOnlyList<StreamConfig> StreamConfigurations => _streamList;

        /// <summary>
        /// Gets or sets the list of custom type handlers for this configuration.
        /// </summary>
        public IReadOnlyList<TypeHandlerConfig> TypeHandlerConfigurations
        {
            get
            {
                return _handlerList;
            }
            set
            {
                _handlerList.Clear();
                if (value != null!)
                    _handlerList.AddRange(value);
            }
        }

        public BeanIOConfig Clone()
        {
            var result = new BeanIOConfig()
                {
                    Source = Source,
                };
            result._streamList.AddRange(_streamList);
            result._handlerList.AddRange(_handlerList);
            return result;
        }

        /// <summary>
        /// Adds a custom type handler to this configuration.
        /// </summary>
        /// <param name="handler">the type handler configuration.</param>
        public void Add(TypeHandlerConfig handler)
        {
            _handlerList.Add(handler);
        }

        /// <summary>
        /// Adds a stream mapping configuration to this configuration.
        /// </summary>
        /// <param name="stream">the stream mapping configuration.</param>
        public void Add(StreamConfig stream)
        {
            _streamList.Add(stream);
        }
    }
}
