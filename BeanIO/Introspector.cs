// <copyright file="Introspector.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO
{
    internal static class Introspector
    {
        /// <summary>
        /// Utility method to take a string and convert it to normal Java variable name capitalization.
        /// </summary>
        /// <remarks>
        ///  This normally means converting the first character from upper case to lower case,
        /// but in the (unusual) special case when there is more than one character and both
        /// the first and second characters are upper case, we leave it alone.
        /// </remarks>
        /// <param name="name">The string to be de-capitalized.</param>
        /// <returns>The de-capitalized version of the string.</returns>
        public static string Decapitalize(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            if (!char.IsUpper(name, 0))
                return name;

            if (name.Length > 1 && char.IsUpper(name, 1))
                return name;

            return string.Format("{0}{1}", char.ToLowerInvariant(name[0]), name.Substring(1));
        }

        public static string Capitalize(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            if (char.IsUpper(name, 0))
                return name;

            return string.Format("{0}{1}", char.ToUpperInvariant(name[0]), name.Substring(1));
        }
    }
}
