// <copyright file="DelimitedParserConfiguration.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace BeanIO.Stream.Delimited
{
    /// <summary>
    /// Stores configuration settings for parsing delimited formatted streams.
    /// </summary>
    public class DelimitedParserConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedParserConfiguration"/> class.
        /// </summary>
        public DelimitedParserConfiguration()
            : this('\t')
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedParserConfiguration"/> class.
        /// </summary>
        /// <param name="delimiter">the field delimiting character.</param>
        public DelimitedParserConfiguration(char delimiter)
        {
            Delimiter = delimiter;
        }

        /// <summary>
        /// Gets or sets the field delimiting character.
        /// </summary>
        /// <remarks>
        /// Defaults to tab.
        /// </remarks>
        public char Delimiter { get; set; }

        /// <summary>
        /// Gets or sets the character used by the input stream to escape delimiters and itself.
        /// </summary>
        public char? Escape { get; set; }

        /// <summary>
        /// Gets or sets the line continuation character or <see langword="null" /> if line continuation is disabled.
        /// </summary>
        public char? LineContinuationCharacter { get; set; }

        /// <summary>
        /// Gets or sets the character used to mark the end of a record.
        /// </summary>
        /// <remarks>
        /// By default, a carriage return (CR), line feed (LF), or CRLF sequence is used to
        /// signify the end of the record.
        /// </remarks>
        public string? RecordTerminator { get; set; }

        /// <summary>
        /// Gets or sets the array of comment prefixes.
        /// </summary>
        /// <remarks>
        /// If a line read from a stream begins with a configured
        /// comment prefix, the line is ignored.  By default, no lines
        /// are considered commented.
        /// </remarks>
        public string[]? Comments { get; set; }

        /// <summary>
        /// Gets a value indicating whether an escape character is enabled.
        /// </summary>
        public bool IsEscapeEnabled => Escape != null;

        /// <summary>
        /// Gets a value indicating whether the line continuation character is enabled.
        /// </summary>
        public bool IsLineContinuationCharacter => LineContinuationCharacter != null;

        /// <summary>
        /// Gets a value indicating whether one or more comment prefixes have been configured.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Comments))]
        public bool IsCommentEnabled => Comments != null && Comments.Length > 0;
    }
}
