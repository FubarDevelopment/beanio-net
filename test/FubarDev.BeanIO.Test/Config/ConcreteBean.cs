// <copyright file="ConcreteBean.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Config
{
    public class ConcreteBean
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string Name { get; private set; }

        public int Age { get; set; }
    }
}
