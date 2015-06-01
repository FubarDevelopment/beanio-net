using System.Collections.Generic;
using System.Xml.Linq;

namespace BeanIO
{
    /// <summary>
    /// Interface for unmarshalling single records.
    /// </summary>
    /// <remarks>
    /// <para>An <see cref="IUnmarshaller"/> can be used to unmarshal a bean object bound to
    /// a <code>record</code> in a mapping file.  Unmarshalling bean objects that span multiple
    /// records is not supported and will cause a <see cref="BeanReaderException"/>.</para>
    /// <para>An <see cref="IUnmarshaller"/> instance is stateful.  If a BeanIO mapping file declares
    /// record ordering and expected occurrences, a <see cref="BeanWriterException"/> may be thrown for
    /// records read out of sequence or that have exceeded their maximum occurrences.</para>
    /// <para>There is some performance benefit for reusing the same <see cref="IUnmarshaller"/> instance,
    /// but an <see cref="IUnmarshaller"/> is not thread safe and should not be used to unmarshal multiple
    /// records concurrently.</para>
    /// </remarks>
    public interface IUnmarshaller
    {
        /// <summary>
        /// Gets the record or group name of the most recent unmarshalled bean object.
        /// </summary>
        string RecordName { get; }

        /// <summary>
        /// Gets record information for the most recent unmarshalled bean object.
        /// </summary>
        IRecordContext RecordContext { get; }

        /// <summary>
        /// Unmarshals a bean object from the given record text.
        /// </summary>
        /// <remarks>
        /// This method is supported by all stream formats.
        /// </remarks>
        /// <param name="record">The record text to unmarshal</param>
        /// <returns>The unmarshalled bean object</returns>
        object Unmarshal(string record);

        /// <summary>
        /// Unmarshals a bean object from the given list of fields.
        /// </summary>
        /// <remarks>
        /// This method is supported by CSV and delimited formatted streams only.
        /// </remarks>
        /// <param name="fields">The fields to unmarshal</param>
        /// <returns>The unmarshalled bean object</returns>
        object Unmarshal(IList<string> fields);

        /// <summary>
        /// Unmarshals a bean object from the given array of fields.
        /// </summary>
        /// <remarks>
        /// This method is supported by CSV and delimited formatted streams only.
        /// </remarks>
        /// <param name="fields">The fields to unmarshal</param>
        /// <returns>The unmarshalled bean object</returns>
        object Unmarshal(string[] fields);

        /// <summary>
        /// Unmarshals a bean object from the given element.
        /// </summary>
        /// <param name="node">The <see cref="XElement"/> to unmarshal</param>
        /// <returns>The unmarshalled bean object</returns>
        object Unmarshal(XElement node);
    }
}
