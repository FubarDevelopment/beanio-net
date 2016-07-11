// <copyright file="VersionTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Types
{
    /// <summary>
    /// Type handler for the <see cref="Version"/> type.
    /// </summary>
    public class VersionTypeHandler : ITypeHandler
    {
        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public Type TargetType => typeof(Version);

        /// <summary>
        /// Gets or sets the number of fields to be used for the version.
        /// </summary>
        public int? FieldCount { get; set; }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public virtual object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            return Version.Parse(text);
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public virtual string Format(object value)
        {
            if (value == null)
                return null;
            var ver = (Version)value;
            if (FieldCount != null)
                return ver.ToString(FieldCount.Value);
            return ver.ToString();
        }
    }
}
