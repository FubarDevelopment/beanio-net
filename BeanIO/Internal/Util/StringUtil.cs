using System.Text;

using AutoMapper.Internal;

namespace BeanIO.Internal.Util
{
    public static class StringUtil
    {
        private static readonly bool LAZY_IF_EMPTY = Settings.Instance.GetBoolean(Settings.LAZY_IF_EMPTY);

        /// <summary>
        /// Returns whether the given object has a value
        /// </summary>
        /// <param name="obj">the object to test</param>
        /// <returns>true if the object is not null (and not the empty string based on configuration)</returns>
        public static bool HasValue(object obj)
        {
            if (obj == null)
                return false;

            if (LAZY_IF_EMPTY && ReferenceEquals(obj, string.Empty))
                return false;

            return true;
        }

        /// <summary>
        /// Substitutes <code>${key,default}</code> place holders with their property values.
        /// </summary>
        /// <param name="text">the template text</param>
        /// <param name="properties">the user provided property values</param>
        /// <returns>the text after property substitution</returns>
        public static string DoPropertySubstitution(string text, IDictionary<string, string> properties)
        {
            return DoPropertySubstitution(text, new DictionaryProperties(properties));
        }

        /// <summary>
        /// Substitutes <code>${key,default}</code> place holders with their property values.
        /// </summary>
        /// <param name="text">the template text</param>
        /// <param name="properties">the user provided property values</param>
        /// <returns>the text after property substitution</returns>
        public static string DoPropertySubstitution(string text, IPropertySource properties)
        {
            if (text == null || text.Length < 3)
                return text;

            var i = text.IndexOf('$');
            if (i < 0)
                return text;

            StringBuilder s = null;
            var state = 1;
            var keyBegin = i;
            var valueBegin = 0;

            ++i;

            var cs = text.ToCharArray();
            for (var j = cs.Length; i < j; ++i)
            {
                var c = cs[i];

                switch (state)
                {
                    case 0:
                        // look for '$'
                        if (c == '$')
                        {
                            keyBegin = i;
                            valueBegin = 0;
                            state = 1;
                        }
                        else if (s != null)
                        {
                            s.Append(c);
                        }
                        break;

                    case 1:
                        // look for '{'
                        if (c == '{')
                        {
                            state = 2;
                        }
                        else
                        {
                            if (s != null)
                                s.Append('$').Append(c);
                            state = 0;
                        }
                        break;

                    case 2:
                        // look for '}'
                        if (c == '}')
                        {
                            var length = valueBegin > 0 ? valueBegin - keyBegin - 2 : i - keyBegin - 2;
                            var key = new string(cs, keyBegin + 2, length);
                            string value = null;
                            if (properties != null)
                                value = properties.GetProperty(key);
                            if (value == null && valueBegin > 0)
                                value = new string(cs, valueBegin + 1, i - valueBegin - 1);
                            if (value == null)
                                throw new BeanIOException(string.Format("Unresolved property '{0}'", key));
                            if (s == null)
                                s = new StringBuilder(new string(cs, 0, keyBegin));
                            s.Append(value);
                            state = 0;
                        }
                        else if (valueBegin == 0 && c == ',')
                        {
                            valueBegin = i;
                        }
                        break;
                }
            }

            if (s != null)
            {
                if (state > 0)
                    s.Append(new string(cs, keyBegin, cs.Length - keyBegin));
                return s.ToString();
            }

            return text;
        }

        private class DictionaryProperties : IPropertySource
        {
            private readonly IDictionary<string, string> _properties;

            public DictionaryProperties(IDictionary<string, string> properties)
            {
                _properties = properties;
            }

            /// <summary>
            /// Returns the property value for a given key
            /// </summary>
            /// <param name="key">the property key</param>
            /// <returns>the property value</returns>
            public string GetProperty(string key)
            {
                if (_properties == null)
                    return null;
                string result;
                if (!_properties.TryGetValue(key, out result))
                    result = null;
                return result;
            }
        }
    }
}
