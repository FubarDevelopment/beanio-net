namespace BeanIO.Stream.Delimited
{
    /// <summary>
    /// Stores configuration settings for parsing delimited formatted streams
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
        /// <param name="delimiter">the field delimiting character</param>
        public DelimitedParserConfiguration(char delimiter)
        {
            Delimiter = delimiter;
        }

        /// <summary>
        /// Gets or sets the field delimiting character
        /// </summary>
        /// <remarks>
        /// Defaults to tab.
        /// </remarks>
        public char Delimiter { get; set; }

        /// <summary>
        /// Gets or sets the character used by the input stream to escape delimiters and itself
        /// </summary>
        public char? Escape { get; set; }

        /// <summary>
        /// Gets or sets the line continuation character or <code>null</code> if line continuation is disabled
        /// </summary>
        public char? LineContinuationCharacter { get; set; }

        /// <summary>
        /// Gets or sets the character used to mark the end of a record
        /// </summary>
        /// <remarks>
        /// By default, a carriage return (CR), line feed (LF), or CRLF sequence is used to
        /// signify the end of the record.
        /// </remarks>
        public string RecordTerminator { get; set; }

        /// <summary>
        /// Gets or sets the array of comment prefixes
        /// </summary>
        /// <remarks>
        /// If a line read from a stream begins with a configured
        /// comment prefix, the line is ignored.  By default, no lines
        /// are considered commented.
        /// </remarks>
        public string[] Comments { get; set; }

        /// <summary>
        /// Gets a value indicating whether an escape character is enabled
        /// </summary>
        public bool IsEscapeEnabled
        {
            get { return Escape != null; }
        }

        /// <summary>
        /// Gets a value indicating whether the line continuation character is enabled
        /// </summary>
        public bool IsLineContinuationCharacter
        {
            get { return LineContinuationCharacter != null; }
        }

        /// <summary>
        /// Gets a value indicating whether one or more comment prefixes have been configured
        /// </summary>
        public bool IsCommentEnabled
        {
            get { return Comments != null && Comments.Length > 0; }
        }
    }
}
