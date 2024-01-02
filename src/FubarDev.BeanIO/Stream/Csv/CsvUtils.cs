// <copyright file="CsvUtils.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace BeanIO.Stream.Csv
{
    internal static class CsvUtils
    {
        /// <summary>
        /// Returns <c>true</c> if the given field must be quoted.
        /// </summary>
        /// <param name="cs">the field to test.</param>
        /// <param name="delim">delimiter character.</param>
        /// <param name="quote">quote character.</param>
        /// <returns><c>true</c> if the given field must be quoted.</returns>
        public static bool MustQuote(this char[] cs, char delim, char quote)
        {
            return cs.Any(x => x == delim || x == quote || x == '\n' || x == '\r');
        }

        public static string ToWhitespace(this int size)
        {
            if (size == 0)
                return string.Empty;
            return new string(' ', size);
        }
    }
}
