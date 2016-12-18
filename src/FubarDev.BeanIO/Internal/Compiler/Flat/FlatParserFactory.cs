// <copyright file="FlatParserFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Config;
using BeanIO.Internal.Config;

namespace BeanIO.Internal.Compiler.Flat
{
    internal abstract class FlatParserFactory : ParserFactorySupport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlatParserFactory"/> class.
        /// </summary>
        /// <param name="settings">The configuration settings</param>
        protected FlatParserFactory(ISettings settings)
            : base(settings)
        {
        }

        /// <summary>
        /// Creates a stream configuration pre-processor
        /// </summary>
        /// <remarks>May be overridden to return a format specific version</remarks>
        /// <param name="config">the stream configuration to pre-process</param>
        /// <returns>the new <see cref="Preprocessor"/></returns>
        protected override Preprocessor CreatePreprocessor(StreamConfig config)
        {
            return new FlatPreprocessor(Settings, config);
        }
    }
}
