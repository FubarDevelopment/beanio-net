using System.Collections.Generic;

namespace BeanIO
{
    public abstract class BeanReader : IBeanReader
    {
        public event BeanReaderErrorHandlerDelegate Error;

        public string RecordName { get; protected set; }

        public int LineNumber { get; protected set; }

        public int RecordCount
        {
            get { return RecordContexts.Count; }
        }

        public abstract IReadOnlyList<IRecordContext> RecordContexts { get; }

        public abstract object Read();

        public abstract int Skip(int count);

        public abstract void Close();

        /// <summary>
        /// Disposes this resource
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Triggers the <see cref="Error" /> event or throws the exception when no event handler was given.
        /// </summary>
        /// <param name="exception">The exception to be passed to the event.</param>
        protected void OnError(BeanReaderException exception)
        {
            if (Error == null)
                throw exception;

            Error(new BeanReaderErrorEventArgs(exception));
        }
    }
}
