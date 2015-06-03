using System;
using System.Linq;
using System.Reflection;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    internal static class ObjectUtils
    {
        public static object NewInstance(Type type)
        {
            if (type == null)
                return null;
            try
            {
                return type.GetTypeInfo().DeclaredConstructors.Single(x => x.GetParameters().Length == 0).Invoke(null);
            }
            catch (Exception ex)
            {
                throw new BeanIOException(string.Format("Failed to instantiate class '{0}'", type.GetFullName()), ex);
            }
        }
    }
}
