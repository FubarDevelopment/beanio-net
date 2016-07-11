// <copyright file="ParsingMode.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// The parsing mode
    /// </summary>
    internal enum ParsingMode
    {
        /// <summary>
        /// Unmarshalling mode
        /// </summary>
        Unmarshalling = 'U',

        /// <summary>
        /// Marshalling mode
        /// </summary>
        Marshalling = 'M',
    }
}
