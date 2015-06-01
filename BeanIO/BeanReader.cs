using System;

namespace BeanIO
{
    public abstract class BeanReader : IBeanReader
    {
        public event BeanReaderErrorHandlerDelegate Error;

        public string RecordName { get; protected set; }

        public int LineNumber { get; protected set; }

        public abstract int RecordCount { get; }

        public abstract IRecordContext GetRecordContext(int index);

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
        protected virtual void OnError(BeanReaderException exception)
        {
            var tmp = Error;

            if (tmp == null)
                throw exception;

            try
            {
                tmp(new BeanReaderErrorEventArgs(exception));
            }
            catch (BeanReaderException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new BeanReaderException("Exception thrown by error handler", e);
            }
        }
    }
}
