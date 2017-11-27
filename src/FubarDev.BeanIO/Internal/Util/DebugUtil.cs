// <copyright file="DebugUtil.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using BeanIO.Internal.Parser.Format;

namespace BeanIO.Internal.Util
{
    internal static class DebugUtil
    {
        public static string FormatRange(int min, int? max)
        {
            if (max == null || max == int.MaxValue)
                return $"{min}+";
            return $"{min}-{max}";
        }

        public static string FormatOption(string option, bool value)
        {
            if (value)
                return option;
            return string.Concat("!", option);
        }

        public static string FormatPadding(this FieldPadding padding)
        {
            if (padding == null)
                return string.Empty;
            return $", padded[length={padding.Length}, filler={padding.Filler}, align={padding.Justify}]";
        }

        public static string ToDebug(this IDebuggable c)
        {
            using (var writer = new StringWriter())
            {
                c.Debug(writer);
                writer.Flush();
                return writer.ToString();
            }
        }

        public static string ToDebug<T>(this IEnumerable<T> items)
        {
            var writer = new StringBuilder();
            var first = true;
            writer.Append("[");
            foreach (var item in items)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    writer.Append(", ");
                }

                var debuggable = item as IDebuggable;
                if (debuggable != null)
                {
                    writer.Append(debuggable.ToDebug());
                }
                else
                {
                    writer.Append(item);
                }
            }

            writer.Append("]");
            return writer.ToString();
        }
    }
}
