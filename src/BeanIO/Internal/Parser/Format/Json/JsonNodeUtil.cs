// <copyright file="JsonNodeUtil.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser.Format.Json
{
    internal static class JsonNodeUtil
    {
        /// <summary>
        /// Returns a description for given JSON node type.
        /// </summary>
        /// <param name="node">the JSON node to return the type description for</param>
        /// <returns>the description</returns>
        public static string GetTypeDescription(this IJsonNode node)
        {
            return $"{node.JsonType}{(node.IsJsonArray ? "[]" : string.Empty)}";
        }
    }
}
