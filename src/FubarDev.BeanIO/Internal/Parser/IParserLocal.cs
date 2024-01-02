// <copyright file="IParserLocal.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// Used to initialize variables held by a <see cref="ParsingContext"/>.
    /// </summary>
    internal interface IParserLocal
    {
        /// <summary>
        /// Initializes the variable.
        /// </summary>
        /// <param name="index">the index of the variable in the <see cref="ParsingContext.LocalHeap"/>.</param>
        /// <param name="context">the <see cref="ParsingContext"/> being initialized.</param>
        void Init(int index, ParsingContext context);
    }
}
