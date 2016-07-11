// <copyright file="IPerson.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using NodaTime;

namespace BeanIO.Parser.WriteMode
{
    public interface IPerson
    {
        string FirstName { get; }

        string LastName { get; }

        int Age { get; }

        LocalDate BirthDate { get; }
    }
}
