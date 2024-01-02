// <copyright file="TestRecord.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BeanIO.Parser.DynamicOccurs
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Set by BeanIO")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Set by BeanIO")]
    public class TestRecord
    {
        public int ItemCount { get; set; }

        public int OtherItemCount { get; set; }

        public TestItem? Item => Items?.SingleOrDefault();

        public TestOtherItem? OtherItem => OtherItems?.SingleOrDefault();

        // ReSharper disable once CollectionNeverUpdated.Local
        private IList<TestItem>? Items { get; set; }

        // ReSharper disable once CollectionNeverUpdated.Local
        private IList<TestOtherItem>? OtherItems { get; set; }
    }
}
