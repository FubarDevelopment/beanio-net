using System;
using System.IO;

using BeanIO.Internal.Parser.Format;

namespace BeanIO.Internal.Util
{
    public static class DebugUtil
    {
        public static string FormatRange(int min, int? max)
        {
            if (max == null || max == int.MaxValue)
                return string.Format("{0}+", min);
            return string.Format("{0}-{1}", min, max);
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
            return string.Format(", padded[length={0}, filler={1}, align={2}]", padding.Length, padding.Filler, padding.Justify);
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
    }
}
