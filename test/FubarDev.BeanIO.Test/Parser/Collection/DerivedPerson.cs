// <copyright file="DerivedPerson.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Beans;

namespace BeanIO.Parser.Collection
{
    public class DerivedPerson : Person
    {
        public string? NickName { get; set; }
    }
}
