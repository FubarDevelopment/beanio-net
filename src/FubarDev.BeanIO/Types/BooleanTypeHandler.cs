// <copyright file="BooleanTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Types
{
    /// <summary>
    /// Type handler for <see cref="bool"/>.
    /// </summary>
    public class BooleanTypeHandler : ITypeHandler
    {
        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public Type TargetType => typeof(bool);

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record.</param>
        /// <returns>The parsed object.</returns>
        public virtual object? Parse(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            return bool.Parse(text!.Trim());
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null.</param>
        /// <returns>The formatted field text, or <see langword="null" /> to indicate the value is not present.</returns>
        public virtual string? Format(object? value)
        {
            return value?.ToString();
        }
    }
}
