using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace BeanIO
{
    public static class ConfigUtil
    {
        public static IDictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            return collection
                .AllKeys.Select(x => new KeyValuePair<string, string>(x, collection[x]))
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
