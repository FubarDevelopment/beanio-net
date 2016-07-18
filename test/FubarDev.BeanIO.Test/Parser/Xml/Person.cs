// <copyright file="Person.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using BeanIO.Internal.Util;

namespace BeanIO.Parser.Xml
{
    public class Person
    {
        public const string DefaultName = "";

        public const int DefaultAge = -1;

        public Person()
        {
            LastName = DefaultName;
            Age = DefaultAge;
        }

        public string Type { get; set; }

        public string Gender { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<string> Color { get; set; }

        public Address Address { get; set; }

        public List<Address> AddressList { get; set; }

        public int? Age { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} {2}: {3} {4}", Gender, FirstName, LastName, Color.ToDebug(), AddressList.ToDebug());
        }
    }
}
