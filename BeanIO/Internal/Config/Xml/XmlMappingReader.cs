using System.Xml;
using System.Xml.Linq;

namespace BeanIO.Internal.Config.Xml
{
    /// <summary>
    /// Reads a BeanIO XML mapping file into an XML document object model (DOM)
    /// </summary>
    public class XmlMappingReader
    {
        /// <summary>
        /// Parses an XML BeanIO mapping file into a document object model (DOM).
        /// </summary>
        /// <param name="input"> input stream to read</param>
        /// <returns>the resulting DOM</returns>
        public virtual XDocument LoadDocument(System.IO.Stream input)
        {
            var readerSettings = new XmlReaderSettings()
                {
                    IgnoreComments = true,
                };
            var reader = XmlReader.Create(input, readerSettings);
            return XDocument.Load(reader);
        }
    }
}
