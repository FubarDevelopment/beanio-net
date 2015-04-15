using System;
using System.Text;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown when a record or one of its fields does not pass validation during unmarshalling.
    /// </summary>
    /// <remarks>
    /// An invalid record does not affect the state of a <see cref="IBeanReader"/>, and subsequent
    /// calls to <see cref="IBeanReader.Read"/> are not affected.
    /// </remarks>
    public class InvalidRecordException : BeanReaderException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRecordException" /> class.
        /// </summary>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public InvalidRecordException(params IRecordContext[] contexts)
            : base(contexts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRecordException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public InvalidRecordException(string message, params IRecordContext[] contexts)
            : base(message, contexts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidRecordException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        /// <param name="contexts">The record context(s) that caused the exception</param>
        public InvalidRecordException(string message, Exception inner, params IRecordContext[] contexts)
            : base(message, inner, contexts)
        {
        }

        /// <summary>
        /// Gets the name of the record or group that failed validation.
        /// </summary>
        public virtual string RecordName
        {
            get
            {
                var ctx = RecordContext;
                return ctx != null ? ctx.RecordName : null;
            }
        }

        public override string ToString()
        {
            var message = base.ToString();
            if (RecordContexts.Count == 0)
                return message;
            var s = new StringBuilder(message);
            return AppendMessageDetails(s).ToString();
        }

        /// <summary>
        /// Called by <see cref="ToString"/> to append record context details to the error message.
        /// </summary>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/> to append to.</param>
        /// <returns>The <see cref="StringBuilder"/> the message was appended to.</returns>
        protected virtual StringBuilder AppendMessageDetails(StringBuilder stringBuilder)
        {
            var context = RecordContext;
            if (context.HasRecordErrors)
            {
                foreach (var error in context.RecordErrors)
                {
                    stringBuilder
                        .AppendLine()
                        .AppendFormat(" ==> {0}", error);
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
                            .AppendFormat(" ==> Invalid '{0}': {1}", fieldName, error);
                    }
                }
            }

            return stringBuilder;
        }
    }
}
