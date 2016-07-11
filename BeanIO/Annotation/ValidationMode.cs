// <copyright file="ValidationMode.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Annotation
{
    /// <summary>
    /// The default validation mode when marshalling fields.
    /// </summary>
    public enum ValidationMode
    {
        /// <summary>
        /// Copy the configuration from the parent element (or Settings)
        /// </summary>
        SameAsParent,

        /// <summary>
        /// Validate on marshal
        /// </summary>
        ValidateOnMarshal,

        /// <summary>
        /// Don't validate on marshal
        /// </summary>
        NoValidateOnMarshal,
    }
}
