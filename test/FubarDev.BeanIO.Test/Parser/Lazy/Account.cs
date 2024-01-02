// <copyright file="Account.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

namespace BeanIO.Parser.Lazy
{
    public class Account
    {
        public int? Number { get; set; }

        public string? Text { get; set; }

        public List<Transaction>? Transactions { get; set; }
    }
}
