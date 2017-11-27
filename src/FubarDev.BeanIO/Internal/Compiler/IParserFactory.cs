// <copyright file="IParserFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Internal.Config;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Compiler
{
    /// <summary>
    /// A <see cref="IParserFactory"/> is used to convert a stream configuration (i.e. <see cref="StreamConfig"/>)
    /// into a stream parser (i.e. <see cref="Parser.Stream"/>).
    /// </summary>
    internal interface IParserFactory
    {
        /// <summary>
        /// Gets or sets the type handler factory to use for resolving type handlers
        /// </summary>
        TypeHandlerFactory TypeHandlerFactory { get; set; }

        /// <summary>
        /// Creates a new stream parser from a given stream configuration
        /// </summary>
        /// <param name="config">the stream configuration</param>
        /// <returns>the created <see cref="Parser.Stream"/></returns>
        Parser.Stream CreateStream(StreamConfig config);
    }
}
