// <copyright file="Value.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser
{
    internal static class Value
    {
        /// <summary>
        /// Constant indicating the field was not present in the stream.
        /// </summary>
        public static readonly string Missing = new string("-missing-".ToCharArray());

        /// <summary>
        /// Constant indicating the field did not pass validation.
        /// </summary>
        public static readonly string Invalid = new string("-invalid-".ToCharArray());

        /// <summary>
        /// Constant indicating the field was nil (XML only).
        /// </summary>
        public static readonly string Nil = new string("-nil-".ToCharArray());
    }
}
