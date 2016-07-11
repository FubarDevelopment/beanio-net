// <copyright file="TestRecordBool.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace BeanIO.Parser.DynamicOccurs
{
    public class TestRecordBool
    {
        public bool HasItem { get; set; }

        public bool HasOtherItem { get; set; }

        public TestItem Item => Items?.SingleOrDefault();

        public TestOtherItem OtherItem => OtherItems?.SingleOrDefault();

        [UsedImplicitly]
        private IList<TestItem> Items { get; set; }

        [UsedImplicitly]
        private IList<TestOtherItem> OtherItems { get; set; }
    }
}
