using System;
using System.IO;
using System.Xml.Linq;

namespace BeanIO.Stream.Xml
{
    /// <summary>
    /// A <see cref="IRecordUnmarshaller"/> implementation for XML formatted records.
    /// </summary>
    public class XmlRecordUnmarshaller : IRecordUnmarshaller
    {
        /// <summary>
        /// Unmarshals a single record.
        /// </summary>
        /// <param name="text">The record text to unmarshal</param>
        /// <returns>The unmarshalled record object</returns>
        public object Unmarshal(string text)
        {
            try
            {
                return XDocument.Load(new StringReader(text), LoadOptions.SetLineInfo);
            }
            catch (Exception ex)
            {
                throw new RecordIOException(ex.Message, ex);
            }
        }
    }
}
