using System;

namespace BeanIO
{
    /// <summary>
    /// Interface for marshalling bean objects to an output stream.
    /// </summary>
    /// <remarks>
    /// A <see cref="IBeanWriter"/> is created using a <see cref="StreamFactory"/> and a mapping file.
    /// </remarks>
    public interface IBeanWriter : IDisposable
    {
        /// <summary>
        /// Writes a bean object to this output stream.
        /// </summary>
        /// <param name="bean">The bean object to write</param>
        void Write(object bean);

        /// <summary>
        /// Writes a bean object to this output stream.
        /// </summary>
        /// <param name="recordName">The record or group name bound to the bean object from the mapping file.</param>
        /// <param name="bean">The bean object to write</param>
        void Write(string recordName, object bean);

        /// <summary>
        /// Flushes this output stream.
        /// </summary>
        void Flush();

        /// <summary>
        /// Closes this output stream.
        /// </summary>
        void Close();
    }
}
