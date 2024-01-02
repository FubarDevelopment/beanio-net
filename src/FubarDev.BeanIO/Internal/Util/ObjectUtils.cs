// <copyright file="ObjectUtils.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;

namespace BeanIO.Internal.Util
{
    internal static class ObjectUtils
    {
        [return: NotNullIfNotNull(nameof(type))]
        public static object? NewInstance(this Type? type)
        {
            if (type == null)
                return null;
            try
            {
                return Activator.CreateInstance(type) ??
                       throw new BeanIOException($"Failed to instantiate class '{type}'");
            }
            catch (Exception ex)
            {
                throw new BeanIOException($"Failed to instantiate class '{type.GetAssemblyQualifiedName()}'", ex);
            }
        }
    }
}
