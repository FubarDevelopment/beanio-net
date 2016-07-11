// <copyright file="DecimalTypeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Types
{
    /// <summary>
    /// Type handler for the <see cref="decimal"/> type.
    /// </summary>
    public class DecimalTypeHandler : NumberTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalTypeHandler"/> class.
        /// </summary>
        public DecimalTypeHandler()
            : base(typeof(decimal))
        {
        }

        /// <summary>
        /// Parses a number from a <see cref="System.Decimal"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Decimal"/> to convert to a number</param>
        /// <returns>The parsed number</returns>
        protected override object CreateNumber(decimal value)
        {
            return value;
        }
    }
}
