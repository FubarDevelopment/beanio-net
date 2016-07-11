// <copyright file="UnboundFieldAttribute.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Annotation
{
    /// <summary>
    /// Annotation used to add fields to a record or segment that are not bound to a class property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class UnboundFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnboundFieldAttribute" /> class.
        /// </summary>
        public UnboundFieldAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnboundFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The field name.</param>
        public UnboundFieldAttribute(string name)
            : base(name)
        {
        }
    }
}
