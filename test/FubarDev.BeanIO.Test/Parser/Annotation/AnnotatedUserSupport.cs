// <copyright file="AnnotatedUserSupport.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Annotation;

namespace BeanIO.Parser.Annotation
{
    public abstract class AnnotatedUserSupport
    {
        [Field(At = 1, IsRequired = true)]
        public string? FirstName { get; set; }

        [Field(At = 2, Getter = "GetSurname", Setter = "SetSurname")]
        public string? LastName { get; set; }

        public string? GetSurname()
        {
            return LastName;
        }

        public void SetSurname(string lastName)
        {
            LastName = lastName;
        }
    }
}
