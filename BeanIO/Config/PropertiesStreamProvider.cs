using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BeanIO.Config
{
    /// <summary>
    /// Reads properties from a stream
    /// </summary>
    public class PropertiesStreamProvider : IPropertiesProvider
    {
        private readonly string _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertiesStreamProvider"/> class.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        public PropertiesStreamProvider(System.IO.Stream stream)
        {
            using (var reader = new StreamReader(stream))
                _content = reader.ReadToEnd();
        }

        /// <summary>
        /// Reads all properties
        /// </summary>
        /// <returns>A dictionary with all properties read</returns>
        public IReadOnlyDictionary<string, string> Read()
        {
            var content = _content;
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = (from l in lines
                          where !l.StartsWith("#")
                          where !l.StartsWith("!")
                          let equalSignIndex = l.IndexOf('=')
                          where equalSignIndex != -1
                          select new KeyValuePair<string, string>(l.Substring(0, equalSignIndex), l.Substring(equalSignIndex + 1)))
                .ToDictionary(x => x.Key.Trim(), x => x.Value.TrimStart());
            return result;
        }
    }
}
