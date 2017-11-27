// <copyright file="IntegerTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Types
{
    /// <summary>
    /// A type handler implementation for the <see cref="int"/> class.
    /// </summary>
    public class IntegerTypeHandler : NumberTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerTypeHandler"/> class.
        /// </summary>
        public IntegerTypeHandler()
            : base(typeof(int))
        {
        }
    }
}
