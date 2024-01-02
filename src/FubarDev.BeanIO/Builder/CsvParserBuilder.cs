// <copyright file="CsvParserBuilder.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Internal.Config;
using BeanIO.Stream;
using BeanIO.Stream.Csv;

namespace BeanIO.Builder
{
    /// <summary>
    /// Builder for CSV parsers.
    /// </summary>
    public class CsvParserBuilder : IParserBuilder
    {
        private readonly CsvRecordParserFactory _parser = new CsvRecordParserFactory();

        /// <summary>
        /// Sets the delimiter character.
        /// </summary>
        /// <param name="delimiter">The delimiter to set.</param>
        /// <returns>the <see cref="CsvParserBuilder"/>.</returns>
        public CsvParserBuilder Delimiter(char delimiter)
        {
            _parser.Delimiter = delimiter;
            return this;
        }

        /// <summary>
        /// Sets the quote character.
        /// </summary>
        /// <param name="quote">The quote character to set.</param>
        /// <returns>the <see cref="CsvParserBuilder"/>.</returns>
        public CsvParserBuilder Quote(char quote)
        {
            _parser.Quote = quote;
            return this;
        }

        /// <summary>
        /// Sets the escape character.
        /// </summary>
        /// <param name="escape">The escape character to set.</param>
        /// <returns>the <see cref="CsvParserBuilder"/>.</returns>
        public CsvParserBuilder Escape(char escape)
        {
            _parser.Escape = escape;
            return this;
        }

        /// <summary>
        /// Sets the record terminator.
        /// </summary>
        /// <param name="terminator">The record terminator to set.</param>
        /// <returns>the <see cref="CsvParserBuilder"/>.</returns>
        public CsvParserBuilder RecordTerminator(string terminator)
        {
            _parser.RecordTerminator = terminator;
            return this;
        }

        /// <summary>
        /// Enables the detection of comments using the following comment indicators.
        /// </summary>
        /// <param name="comments">The comment indicators to set.</param>
        /// <returns>the <see cref="CsvParserBuilder"/>.</returns>
        public CsvParserBuilder EnableComments(params string[] comments)
        {
            _parser.Comments = comments;
            return this;
        }

        /// <summary>
        /// Enables multi line records.
        /// </summary>
        /// <returns>the <see cref="CsvParserBuilder"/>.</returns>
        public CsvParserBuilder EnableMultiline()
        {
            _parser.IsMultilineEnabled = true;
            return this;
        }

        /// <summary>
        /// Allow unquoted whitespace characters.
        /// </summary>
        /// <returns>the <see cref="CsvParserBuilder"/>.</returns>
        public CsvParserBuilder AllowUnquotedWhitespace()
        {
            _parser.IsWhitespaceAllowed = true;
            return this;
        }

        /// <summary>
        /// Allow unquoted quotes.
        /// </summary>
        /// <returns>the <see cref="CsvParserBuilder"/>.</returns>
        public CsvParserBuilder AllowUnquotedQuotes()
        {
            _parser.UnquotedQuotesAllowed = true;
            return this;
        }

        /// <summary>
        /// Always quote all field values.
        /// </summary>
        /// <returns>the <see cref="CsvParserBuilder"/>.</returns>
        public CsvParserBuilder AlwaysQuote()
        {
            _parser.AlwaysQuote = true;
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
