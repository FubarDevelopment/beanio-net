namespace BeanIO.Stream.FixedLength
{
    /// <summary>
    /// A combined <see cref="IRecordMarshaller"/> and <see cref="IRecordUnmarshaller"/> implementation
    /// for fixed length formatted records.
    /// </summary>
    public class FixedLengthRecordParser : IRecordMarshaller, IRecordUnmarshaller
    {
        /// <summary>
        /// Marshals a single record object to a <code>String</code>.
        /// </summary>
        /// <param name="record">Record the record object to marshal</param>
        /// <returns>The marshalled record text</returns>
        public string Marshal(object record)
        {
            return Marshal((string)record);
        }

        /// <summary>
        /// Unmarshals a single record.
        /// </summary>
        /// <param name="text">The record text to unmarshal</param>
        /// <returns>The unmarshalled record object</returns>
        public object Unmarshal(string text)
        {
            return text;
        }

        /// <summary>
        /// Marshals a single record object to a <code>String</code>.
        /// </summary>
        /// <param name="record">Record the record object to marshal</param>
        /// <returns>The marshalled record text</returns>
        public string Marshal(string record)
        {
            return record;
        }
    }
}
