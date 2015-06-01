using System;
using System.Collections.Generic;

namespace BeanIO.Internal
{
    internal static class DictionaryExtensions
    {
        public static object Get(this IDictionary<string, object> dictionary, string key)
        {
            return Get(dictionary, key, null);
        }

        public static object Get(this IDictionary<string, object> dictionary, string key, object defaultValue)
        {
            object temp;
            if (dictionary.TryGetValue(key, out temp))
                return temp;
            return defaultValue;
        }
    }
}
