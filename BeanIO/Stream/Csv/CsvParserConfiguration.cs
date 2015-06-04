﻿using System;

namespace BeanIO.Stream.Csv
{
    public class CsvParserConfiguration
    {
        public CsvParserConfiguration()
        {
            Delimiter = ',';
            Quote = '"';
            Escape = '"';
        }

        /// <summary>
        /// Gets or sets the field delimiter. By default, the delimiter is a comma.
        /// </summary>
        public char Delimiter { get; set; }

        /// <summary>
        /// Gets or sets the character to use for a quotation mark. Defaults to '"'.
        /// </summary>
        public char Quote { get; set; }

        /// <summary>
        /// Gets or sets the escape character.
        /// </summary>
        /// <remarks>
        /// Quotation marks can be escaped within quoted
        /// values using the escape character. For example, using the default escape
        /// character, '"Hello ""friend"""' is parsed into 'Hello "friend"'.  Set
        /// to <code>null</code> to disable escaping.
        /// </remarks>
        public char? Escape { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a record may span multiple lines (when quoted).
        /// </summary>
        public bool IsMultilineEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unquoted whitespace is ignored.
        /// </summary>
        public bool IsWhitespaceAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quotes are allowed to appear in an unquoted field
        /// </summary>
        /// <remarks>
        /// Set to <code>false</code> by default which will cause the following record to throw an exception:
        /// <code>
        /// Field1,Field"2,Field3
        /// </code>
        /// </remarks>
        public bool UnquotedQuotesAllowed { get; set; }

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
        /// Gets or sets the text used to terminate a record
        /// </summary>
        /// <remarks>
        /// By default, the record terminator is set to the value of the <see cref="Environment.NewLine"/> system property.
        /// </remarks>
        public string RecordTerminator { get; set; }

        /// <summary>
        /// Gets or sets the text used to terminate a record
        /// </summary>
        /// <remarks>
        /// By default, the line separator is set using the 'line.separator' system property.
        /// </remarks>
        public string LineSeparator
        {
            get { return RecordTerminator; }
            set { RecordTerminator = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether fields should always be quoted when marshalled
        /// </summary>
        /// <remarks>
        /// Defaults to <code>false</code> which will only quote fields containing a quotation mark,
        /// delimiter, line feeds or carriage return
        /// </remarks>
        public bool AlwaysQuote { get; set; }

        /// <summary>
        /// Gets a value indicating whether escaping is enabled. By default, escaping is enabled.
        /// </summary>
        public bool IsEscapeEnabled
        {
            get { return Escape != null; }
        }

        /// <summary>
        /// Gets a value indicating whether one or more comment prefixes have been configured.
        /// </summary>
        public bool IsCommentEnabled
        {
            get { return Comments != null && Comments.Length > 0; }
        }
    }
}