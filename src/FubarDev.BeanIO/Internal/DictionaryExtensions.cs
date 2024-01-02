// <copyright file="DictionaryExtensions.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace BeanIO.Internal
{
    internal static class DictionaryExtensions
    {
        public static object? Get(this IReadOnlyDictionary<string, object?> dictionary, string key)
        {
            return Get(dictionary, key, null);
        }

        public static object? Get(this IReadOnlyDictionary<string, object?> dictionary, string key, object? defaultValue)
        {
            if (dictionary.TryGetValue(key, out var temp))
                return temp;
            return defaultValue;
        }
    }
}
