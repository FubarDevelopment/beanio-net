// <copyright file="User.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Parser.Property
{
    public class User
    {
        private int _type;

        public string? Name { get; set; }

        public new int GetType()
        {
            return _type;
        }

        public void SetType(int type)
        {
            _type = type;
        }
    }
}
