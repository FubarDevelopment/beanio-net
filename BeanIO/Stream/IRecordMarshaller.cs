namespace BeanIO.Stream
{
    /// <summary>
    /// Interface for marshalling a single record object.
    /// </summary>
    /// <remarks>
    /// The class used to represent a <i>record</i> is specific to the
    /// format of a record.  For example, a delimited record marshaller may use
    /// <code>String[]</code>.
    /// </remarks>
    public interface IRecordMarshaller
    {
        /// <summary>
        /// Marshals a single record object to a <code>String</code>.
        /// </summary>
        /// <param name="record">Record the record object to marshal</param>
        /// <returns>The marshalled record text</returns>
        string Marshal(object record);
    }
}
