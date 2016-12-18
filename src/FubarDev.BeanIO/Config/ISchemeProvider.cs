// <copyright file="ISchemeProvider.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

namespace BeanIO.Config
{
    public interface ISchemeProvider
    {
        IEnumerable<string> SupportedSchemes { get; }
        ISchemeHandler GetSchemeHandler(string scheme, bool throwIfMissing);
    }
}