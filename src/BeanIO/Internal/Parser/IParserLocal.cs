// <copyright file="IParserLocal.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser
{
    internal interface IParserLocal
    {
        void Init(int index, ParsingContext context);
    }
}
