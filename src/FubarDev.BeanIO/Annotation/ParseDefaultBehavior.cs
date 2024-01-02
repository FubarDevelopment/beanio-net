// <copyright file="ParseDefaultBehavior.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Annotation
{
    /// <summary>
    /// The behavior for parsing the default value during configuration.
    /// </summary>
    public enum ParseDefaultBehavior
    {
        /// <summary>
        /// As configured in the <c>beanio.properties</c> (default: <c>Unmarshal</c>).
        /// </summary>
        Default,

        /// <summary>
        /// Unmarshal the default value during configuration.
        /// </summary>
        Parse,

        /// <summary>
        /// Keep the default value as text.
        /// </summary>
        DontParse,
    }
}
