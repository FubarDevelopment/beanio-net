// <copyright file="FixedLengthParserFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Internal.Compiler.Flat;
using BeanIO.Internal.Config;
using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Format.FixedLength;
using BeanIO.Stream;
using BeanIO.Stream.FixedLength;

namespace BeanIO.Internal.Compiler.FixedLength
{
    internal class FixedLengthParserFactory : FlatParserFactory
    {
        /// <summary>
        /// Creates the default <see cref="IRecordParserFactory"/>.
        /// </summary>
        /// <returns>
        /// The new <see cref="IRecordParserFactory"/>
        /// </returns>
        protected override IRecordParserFactory CreateDefaultRecordParserFactory()
        {
            return new FixedLengthRecordParserFactory();
        }

        protected override IStreamFormat CreateStreamFormat(StreamConfig config)
        {
            var format = new FixedLengthStreamFormat()
                {
                    Name = config.Name,
                    RecordParserFactory = CreateRecordParserFactory(config),
                };
            return format;
        }

        protected override IRecordFormat CreateRecordFormat(RecordConfig config)
        {
            var format = new FixedLengthRecordFormat();
            if (config.MinLength != null)
                format.MinLength = config.MinLength.Value;
            if (config.MaxLength != null)
                format.MaxLength = config.MaxLength.Value;
            if (config.MinMatchLength != null)
                format.MinMatchLength = config.MinMatchLength.Value;
            if (config.MaxMatchLength != null)
                format.MaxMatchLength = config.MaxMatchLength.Value;
            return format;
        }

        protected override IFieldFormat CreateFieldFormat(FieldConfig config, Type type)
        {
            var padding = new FixedLengthFieldPadding()
            {
                Length = config.Length ?? 0,
                Filler = config.Padding ?? ' ',
                Justify = config.Justify,
                IsOptional = !config.IsRequired,
                PropertyType = type,
            };
            padding.Init();

            var format = new FixedLengthFieldFormat()
                {
                    Name = config.Name,
                    Position = config.Position ?? 0,
                    Until = config.Until ?? 0,
                    IsLazy = config.MinOccurs == 0,
                    KeepPadding = config.KeepPadding,
                    IsLenientPadding = config.IsLenientPadding,
                    Padding = padding,
                };

            return format;
        }

        /// <summary>
        /// Creates a stream configuration pre-processor
        /// </summary>
        /// <remarks>May be overridden to return a format specific version</remarks>
        /// <param name="config">the stream configuration to pre-process</param>
        /// <returns>the new <see cref="Preprocessor"/></returns>
        protected override Preprocessor CreatePreprocessor(StreamConfig config)
        {
            return new FixedLengthPreprocessor(config);
        }
    }
}
