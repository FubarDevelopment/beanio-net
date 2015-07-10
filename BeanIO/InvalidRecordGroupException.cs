using System;
using System.Linq;
using System.Text;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown when one or more records fail validation while unmarshalling a record group.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="InvalidRecordGroupException.RecordName"/> property will return the name
    /// of the group (from the mapping file) that failed validation.</para>
    /// <para>An invalid record group does not affect the state of a <see cref="IBeanReader"/>, and subsequent
    /// calls to <see cref="IBeanReader.Read"/> are not affected.</para>
    /// </remarks>
    public class InvalidRecordGroupException : InvalidRecordException
    {
        private readonly string _groupName;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRecordGroupException" /> class.
        /// </summary>
        /// <param name="groupName">The group name</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public InvalidRecordGroupException(string groupName, params IRecordContext[] contexts)
            : base(contexts)
        {
            _groupName = groupName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRecordGroupException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="groupName">The group name</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public InvalidRecordGroupException(string message, string groupName, params IRecordContext[] contexts)
            : base(message, contexts)
        {
            _groupName = groupName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRecordGroupException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        /// <param name="groupName">The group name</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public InvalidRecordGroupException(string message, Exception inner, string groupName, params IRecordContext[] contexts)
            : base(message, inner, contexts)
        {
            _groupName = groupName;
        }

        /// <summary>
        /// Gets the record group name.
        /// </summary>
        public override string RecordName
        {
            get { return _groupName; }
        }

        /// <summary>
        /// Called by <see cref="InvalidRecordException.ToString"/> to append record context details to the error message.
        /// </summary>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/> to append to.</param>
        /// <returns>The <see cref="StringBuilder"/> the message was appended to.</returns>
        protected override StringBuilder AppendMessageDetails(StringBuilder stringBuilder)
        {
            foreach (var context in RecordContexts.Where(x => x.HasErrors))
            {
                stringBuilder
                    .AppendLine()
                    .AppendFormat(" ==> Invalid '{0}' record at line {1}", context.RecordName, context.LineNumber);

                if (context.HasRecordErrors)
                {
                    foreach (var error in context.RecordErrors)
                    {
                        stringBuilder
                            .AppendLine()
                            .AppendFormat("     - {0}", error);
                    }
                }

                if (context.HasFieldErrors)
                {
                    foreach (var fieldError in context.GetFieldErrors())
                    {
                        var fieldName = fieldError.Key;
                        foreach (var error in fieldError)
                        {
                            stringBuilder
                                .AppendLine()
                                .AppendFormat("     - Invalid '{0}': {1}", fieldName, error);
                        }
                    }
                }
            }

            return stringBuilder;
        }
    }
}
