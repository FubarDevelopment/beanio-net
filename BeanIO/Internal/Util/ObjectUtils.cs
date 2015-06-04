using System;
using System.Linq;
using System.Reflection;

namespace BeanIO.Internal.Util
{
    internal static class ObjectUtils
    {
        private static readonly MethodInfo _getDefaultGenericMethodInfo;

        static ObjectUtils()
        {
            _getDefaultGenericMethodInfo = typeof(ObjectUtils).GetRuntimeMethod("GetDefaultGeneric", new Type[0]);
        }

        public static object NewInstance(this Type type)
        {
            if (type == null)
                return null;
            try
            {
                var constructor = type.GetTypeInfo().DeclaredConstructors.SingleOrDefault(x => x.GetParameters().Length == 0);
                if (constructor == null)
                    return _getDefaultGenericMethodInfo.MakeGenericMethod(type).Invoke(null, null);
                return type.GetTypeInfo().DeclaredConstructors.Single(x => x.GetParameters().Length == 0).Invoke(null);
            }
            catch (Exception ex)
            {
                throw new BeanIOException(string.Format("Failed to instantiate class '{0}'", type.GetFullName()), ex);
            }
        }

        public static T GetDefaultGeneric<T>()
        {
            return default(T);
        }
    }
}
