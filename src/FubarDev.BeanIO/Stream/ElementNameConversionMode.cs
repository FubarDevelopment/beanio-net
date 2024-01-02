// <copyright file="ElementNameConversionMode.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Stream
{
    /// <summary>
    /// Defines the way the element name is produced when based off a property or field name.
    /// </summary>
    [Flags]
    public enum ElementNameConversionMode
    {
        /// <summary>
        /// No changes to the name.
        /// </summary>
        Unchanged = 0,

        /// <summary>
        /// Change the first names character to a lower case character.
        /// </summary>
        Decapitalize = 1,

        /// <summary>
        /// Capitalize the name.
        /// </summary>
        Capitalize = 2,

        /// <summary>
        /// Convert the name to contain only lower case characters.
        /// </summary>
        AllLower = 3,

        /// <summary>
        /// Convert the name to contain only upper case characters.
        /// </summary>
        AllUpper = 4,

        /// <summary>
        /// The mask for all flags that influence the name case conversion.
        /// </summary>
        CasingMask = 15,

        /// <summary>
        /// Remove the underscore from the beginning of the name.
        /// </summary>
        RemoveUnderscore = 16,
    }
}
