// <copyright file="ObjectUtils.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

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
                throw new BeanIOException($"Failed to instantiate class '{type.GetAssemblyQualifiedName()}'", ex);
            }
        }

        public static object NewInstance(this Type type, IReadOnlyCollection<object> availableObjects)
        {
            if (type == null)
                return null;

            var match = BeanUtil.GetBestConstructorMatch(type, availableObjects);
            if (match == null)
                return type.NewInstance();

            try
            {
                return match.Constructor.Invoke(match.Arguments.ToArray());
            }
            catch (Exception ex)
            {
                throw new BeanIOException($"Failed to instantiate class '{type.GetAssemblyQualifiedName()}'", ex);
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
