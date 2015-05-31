using System;
using System.Linq;

using BeanIO.Internal.Util;

namespace BeanIO.Stream.Util
{
    /// <summary>
    /// Skips commented lines read from an input stream
    /// </summary>
    /// <remarks>
    /// A line is considered commented if it starts with one
    /// of the configured comment indicators.
    /// </remarks>
    public class CommentReader
    {
        private readonly MarkableTextReader _in;

        private readonly string[] _comments;

        private readonly char? _recordTerminator;

        private readonly char[] _commentBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentReader"/> class.
        /// </summary>
        /// <param name="reader">the input stream to read from</param>
        /// <param name="comments">an array of comment identifying strings</param>
        public CommentReader(MarkableTextReader reader, string[] comments)
            : this(reader, comments, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentReader"/> class.
        /// </summary>
        /// <param name="reader">the input stream to read from</param>
        /// <param name="comments">an array of comment identifying strings</param>
        /// <param name="recordTerminator">the record terminating character</param>
        public CommentReader(MarkableTextReader reader, string[] comments, char? recordTerminator)
        {
            if (comments == null)
                throw new ArgumentNullException("comments", "Comments not set");
            if (reader == null)
                throw new ArgumentNullException("reader", "Reader not set");

            _in = reader;
            _comments = comments;
            _recordTerminator = recordTerminator;

            var maximumCommentLength = _comments.Where(x => x != null).Max(x => x.Length);
            _commentBuffer = new char[maximumCommentLength];
        }

        /// <summary>
        /// Gets a value indicating whether the next character should be ignored if its a line feed
        /// </summary>
        public bool SkipLineFeed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the end of the stream was reached reading a commented line
        /// </summary>
        public bool IsEof { get; private set; }

        /// <summary>
        /// Skips comments in the input stream and returns the number of commented lines read
        /// </summary>
        /// <remarks>
        /// If no commented lines were read, the stream is positioned just as it had been before this method is called
        /// </remarks>
        /// <param name="initialSkipLineFeed">true if the first line feed character read should be ignored</param>
        /// <returns>the number of skipped comment lines</returns>
        public int SkipComments(bool initialSkipLineFeed)
        {
            SkipLineFeed = initialSkipLineFeed;
            var lines = 0;
            while (true)
            {
                // mark our current position in the stream
                _in.Mark(_commentBuffer.Length);

                // read the start of the line
                int n = _in.ReadBlock(_commentBuffer, 0, _commentBuffer.Length);
                if (n <= 0)
                    break;

                string linePrefix;
                if (SkipLineFeed && _commentBuffer[0] == '\n')
                {
                    linePrefix = new string(_commentBuffer, 1, n - 1);
                }
                else
                {
                    linePrefix = new string(_commentBuffer, 0, n);
                }

                // determine if the line prefix matches a configured comment
                var commentFound = false;
                foreach (var s in _comments)
                {
                    if (linePrefix.StartsWith(s, StringComparison.Ordinal))
                    {
                        commentFound = true;
                        ++lines;
                        break;
                    }
                }

                // if no comment was found, break out
                if (!commentFound)
                    break;

                // finish reading the entire line
                _in.Reset();
                while ((n = _in.Read()) != -1)
                {
                    char c = char.ConvertFromUtf32(n)[0];

                    if (_recordTerminator == null)
                    {
                        if (SkipLineFeed)
                        {
                            SkipLineFeed = false;
                            if (c == '\n')
                                continue;
                        }

                        if (c == '\n')
                            break;

                        if (c == '\r')
                        {
                            SkipLineFeed = true;
                            break;
                        }
                    }
                    else if (c == _recordTerminator)
                    {
                        break;
                    }
                }

                if (n == -1)
                {
                    IsEof = true;
                    return lines;
                }
            }

            _in.Reset();
            return lines;
        }
    }
}
