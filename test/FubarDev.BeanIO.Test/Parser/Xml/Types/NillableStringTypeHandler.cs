// <copyright file="NillableStringTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Internal.Parser;
using BeanIO.Types;

namespace BeanIO.Parser.Xml.Types
{
    public class NillableStringTypeHandler : StringTypeHandler
    {
        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public override string Format(object value)
        {
            if (string.Equals("nil", value))
                return Value.Nil;
            return base.Format(value);
        }
    }
}
