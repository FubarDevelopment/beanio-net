// <copyright file="DelimitedParserBuilder.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Internal.Config;
using BeanIO.Stream;
using BeanIO.Stream.Delimited;

namespace BeanIO.Builder
{
    /// <summary>
    /// Builder for delimited parsers.
    /// </summary>
    public class DelimitedParserBuilder : IParserBuilder
    {
        private readonly DelimitedRecordParserFactory _parser = new DelimitedRecordParserFactory();

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedParserBuilder"/> class.
        /// </summary>
        public DelimitedParserBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedParserBuilder"/> class.
        /// </summary>
        /// <param name="delimiter">The field delimiter to use</param>
        public DelimitedParserBuilder(char delimiter)
        {
            _parser.Delimiter = delimiter;
        }

        /// <summary>
        /// Sets the field delimiter character
        /// </summary>
        /// <param name="delimiter">The field delimiter character to set</param>
        /// <returns>the <see cref="DelimitedParserBuilder"/></returns>
        public DelimitedParserBuilder Delimiter(char delimiter)
        {
            _parser.Delimiter = delimiter;
            return this;
        }

        /// <summary>
        /// Sets the record terminator string
        /// </summary>
        /// <param name="terminator">The record terminator to set</param>
        /// <returns>the <see cref="DelimitedParserBuilder"/></returns>
        public DelimitedParserBuilder RecordTerminator(string terminator)
        {
            _parser.RecordTerminator = terminator;
            return this;
        }

        /// <summary>
        /// Sets the escape character.
        /// </summary>
        /// <param name="escape">The escape character to use</param>
        /// <returns>the <see cref="DelimitedParserBuilder"/></returns>
        public DelimitedParserBuilder EnableEscape(char escape)
        {
            _parser.Escape = escape;
            return this;
        }

        /// <summary>
        /// Enable line continuation using the following escape character
        /// </summary>
        /// <param name="c">The line continuation escape character to use</param>
        /// <returns>the <see cref="DelimitedParserBuilder"/></returns>
        public DelimitedParserBuilder EnableLineContinuation(char c)
        {
            _parser.LineContinuationCharacter = c;
            return this;
        }

        /// <summary>
        /// Enables the detection of comments using the following comment indicators
        /// </summary>
        /// <param name="comments">The comment indicators to set</param>
        /// <returns>the <see cref="DelimitedParserBuilder"/></returns>
        public DelimitedParserBuilder EnableComments(params string[] comments)
        {
            _parser.Comments = comments;
            return this;
        }

        /// <summary>
        /// Builds the configuration about the record parser factory.
        /// </summary>
        /// <returns>The configuration for the record parser factory.</returns>
        public BeanConfig<IRecordParserFactory> Build()
        {
            var config = new BeanConfig<IRecordParserFactory>(() => _parser);
            return config;
        }
    }
}
