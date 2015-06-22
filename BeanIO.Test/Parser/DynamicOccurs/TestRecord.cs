﻿using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace BeanIO.Parser.DynamicOccurs
{
    public class TestRecord
    {
        public int ItemCount { get; set; }

        public int OtherItemCount { get; set; }

        public TestItem Item
        {
            get { return Items == null ? null : Items.SingleOrDefault(); }
        }

        public TestOtherItem OtherItem
        {
            get { return OtherItems == null ? null : OtherItems.SingleOrDefault(); }
        }

        private IList<TestItem> Items { get; [UsedImplicitly] set; }

        private IList<TestOtherItem> OtherItems { get; [UsedImplicitly] set; }
    }
}