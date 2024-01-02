// <copyright file="ComponentType.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// Component types.
    /// </summary>
    public enum ComponentType
    {
        /// <summary>
        /// Group component type.
        /// </summary>
        Group = 'G',

        /// <summary>
        /// Record component type.
        /// </summary>
        Record = 'R',

        /// <summary>
        /// Segment component type.
        /// </summary>
        Segment = 'S',

        /// <summary>
        /// Field component type.
        /// </summary>
        Field = 'F',

        /// <summary>
        /// Constant component type.
        /// </summary>
        Constant = 'C',

        /// <summary>
        /// Wrapper component type.
        /// </summary>
        Wrapper = 'W',

        /// <summary>
        /// Stream component type.
        /// </summary>
        Stream = 'M',
    }
}
