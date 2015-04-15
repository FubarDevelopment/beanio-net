using System;

namespace BeanIO
{
    public class BeanReaderErrorEventArgs : EventArgs
    {
        public BeanReaderErrorEventArgs(BeanReaderException exception)
        {
            Exception = exception;
        }

        public BeanReaderException Exception { get; private set; }
    }
}
