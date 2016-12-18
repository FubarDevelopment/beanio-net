// <copyright file="CsvParserFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Config;
using BeanIO.Internal.Compiler.Delimited;
using BeanIO.Internal.Config;
using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Format.Csv;
using BeanIO.Stream;
using BeanIO.Stream.Csv;

namespace BeanIO.Internal.Compiler.Csv
{
    internal class CsvParserFactory : DelimitedParserFactory
    {
        public CsvParserFactory(ISettings settings)
            : base(settings)
        {
        }

        protected override IStreamFormat CreateStreamFormat(StreamConfig config)
        {
            var format = new CsvStreamFormat()
                {
                    Name = config.Name,
                    RecordParserFactory = CreateRecordParserFactory(config),
                };
            return format;
        }

        /// <summary>
        /// Creates the default <see cref="IRecordParserFactory"/>.
        /// </summary>
        /// <returns>
        /// The new <see cref="IRecordParserFactory"/>
        /// </returns>
        protected override IRecordParserFactory CreateDefaultRecordParserFactory()
        {
            return new CsvRecordParserFactory();
        }
    }
}
