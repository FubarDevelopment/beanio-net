// <copyright file="InvalidBeanException.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO
{
    /// <summary>
    /// Exception thrown by a <see cref="IBeanWriter"/> or <see cref="IMarshaller"/>, when a bean
    /// cannot be marshalled to meet the configured field validation rules.
    /// </summary>
    public class InvalidBeanException : BeanWriterException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBeanException"/> class.
        /// </summary>
        public InvalidBeanException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBeanException"/> class.
        /// </summary>
        /// <param name="message">the error message</param>
        public InvalidBeanException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBeanException"/> class.
        /// </summary>
        /// <param name="message">the error message</param>
        /// <param name="innerException">the root cause</param>
        public InvalidBeanException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
