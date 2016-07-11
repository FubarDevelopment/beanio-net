// <copyright file="TypeConversionException.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Types
{
    /// <summary>
    /// Thrown when field text cannot be parsed into a value object.
    /// </summary>
    public class TypeConversionException : BeanIOException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConversionException" /> class.
        /// </summary>
        public TypeConversionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConversionException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public TypeConversionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeConversionException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception</param>
        public TypeConversionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
