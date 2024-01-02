// <copyright file="Address.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Parser.Xml
{
    public class Address
    {
        public string? City { get; set; }

        public string? State { get; set; }

        public string? Zip { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", City, State, Zip);
        }
    }
}
