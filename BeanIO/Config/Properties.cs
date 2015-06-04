using System.Collections;
using System.Collections.Generic;

namespace BeanIO.Config
{
    /// <summary>
    /// Replacement for Java Properties
    /// </summary>
    public class Properties : IReadOnlyDictionary<string, string>
    {
        private readonly IReadOnlyDictionary<string, string> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="Properties"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary to use to provide the properties</param>
        public Properties(IReadOnlyDictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public IEnumerable<string> Keys
        {
            get { return _dictionary.Keys; }
        }

        public IEnumerable<string> Values
        {
            get { return _dictionary.Values; }
        }

        public string this[string key]
        {
            get
            {
                string result;
                if (!_dictionary.TryGetValue(key, out result))
                    result = null;
                return result;
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}
