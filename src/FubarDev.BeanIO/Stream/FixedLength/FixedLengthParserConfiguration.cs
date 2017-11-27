// <copyright file="FixedLengthParserConfiguration.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Stream.FixedLength
{
    /// <summary>
    /// Stores configuration settings for parsing fixed length formatted streams.
    /// </summary>
    public class FixedLengthParserConfiguration
    {
        /// <summary>
        /// Gets or sets the line continuation character
        /// </summary>
        /// <remarks>By default, line continuation is disabled and null is returned.</remarks>
        public char? LineContinuationCharacter { get; set; }

        /// <summary>
        /// Gets or sets the character used to mark the end of a record
        /// </summary>
        /// <remarks>
        /// By default, a carriage return (CR), line feed (LF), or CRLF sequence is used to signify the end of the record.
        /// </remarks>
        public string RecordTerminator { get; set; }

        /// <summary>
        /// Gets or sets the array of comment prefixes
        /// </summary>
        /// <remarks>
        /// If a line read from a stream begins with a configured comment
        /// prefix, the line is ignored.  By default, no lines are considered
        /// commented.
        /// </remarks>
        public string[] Comments { get; set; }

        /// <summary>
        /// Gets a value indicating whether whether one or more comment prefixes have been configured.
        /// </summary>
        public bool IsCommentEnabled => Comments != null && Comments.Length > 0;

        /// <summary>
        /// Gets a value indicating whether the line continuation character is enabled
        /// </summary>
        public bool IsLineContinationEnabled => LineContinuationCharacter != null;
    }
}
