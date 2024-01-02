// <copyright file="FlatParserFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Internal.Config;

namespace BeanIO.Internal.Compiler.Flat
{
    internal abstract class FlatParserFactory : ParserFactorySupport
    {
        /// <summary>
        /// Creates a stream configuration pre-processor.
        /// </summary>
        /// <remarks>May be overridden to return a format specific version</remarks>
        /// <param name="config">the stream configuration to pre-process.</param>
        /// <returns>the new <see cref="Preprocessor"/>.</returns>
        protected override Preprocessor CreatePreprocessor(StreamConfig config)
        {
            return new FlatPreprocessor(config);
        }
    }
}
