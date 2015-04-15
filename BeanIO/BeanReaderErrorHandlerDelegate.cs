namespace BeanIO
{
    /// <summary>
    /// A delegate for handling exceptions thrown by a <see cref="IBeanReader"/>.
    /// </summary>
    /// <remarks>
    /// The exception will be thrown again (probably as new <see cref="BeanReaderException"/>) when
    /// no error handler is set for the <see cref="IBeanReader.Error"/> event.
    /// </remarks>
    /// <param name="args">The event arguments containing a reference to the <see cref="BeanReaderException"/>.</param>
    public delegate void BeanReaderErrorHandlerDelegate(BeanReaderErrorEventArgs args);
}
