// <copyright file="LazyUser.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BeanIO.Parser.Lazy
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Java beanio compatibility")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules", Justification = "Reviewed. Suppression is OK here.")]
    public class LazyUser
    {
        public string? name;

        public Account? account;

        public List<Account>? accounts;
    }
}
