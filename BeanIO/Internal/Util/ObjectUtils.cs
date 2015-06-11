using System;
using System.Collections.Generic;
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
                var constructor = type.GetTypeInfo().DeclaredConstructors.SingleOrDefault(x => !x.IsStatic && x.GetParameters().Length == 0);
                if (constructor == null)
                    return _getDefaultGenericMethodInfo.MakeGenericMethod(type).Invoke(null, null);
                return constructor.Invoke(null);
            }
            catch (Exception ex)
            {
                throw new BeanIOException(string.Format("Failed to instantiate class '{0}'", type.GetAssemblyQualifiedName()), ex);
            }
        }

        public static T GetDefaultGeneric<T>()
        {
            return default(T);
        }

        public static T[] Empty<T>()
        {
            // Note that the static type is only instantiated when
            // it is needed, and only then is the T[0] object created, once.
            return EmptyArray<T>.Instance;
        }

        private static class EmptyArray<T>
        {
            public static readonly T[] Instance = new T[0];
        }
    }
}
