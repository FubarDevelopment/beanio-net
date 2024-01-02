// <copyright file="ITypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Types
{
    /// <summary>
    /// A <c>TypeHandler</c> is used to convert field text into a Java object and vice versa.
    /// </summary>
    /// <remarks>
    /// Implementations should be thread-safe if multiple threads may concurrently process the
    /// same stream type.  All included BeanIO type handlers are thread safe.
    /// </remarks>
    public interface ITypeHandler
    {
        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record.</param>
        /// <returns>The parsed object.</returns>
        object? Parse(string? text);

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null.</param>
        /// <returns>The formatted field text, or <see langword="null" /> to indicate the value is not present.</returns>
        string? Format(object? value);
    }
}
