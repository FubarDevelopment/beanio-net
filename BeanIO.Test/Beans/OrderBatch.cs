﻿using System.Collections.Generic;
using System.Linq;

namespace BeanIO.Beans
{
    public class OrderBatch
    {
        public int BatchCount { get; set; }

        public Order Order { get; set; }

        public List<Order> Orders { get; set; }

        public Order[] GetOrderArray()
        {
            return Orders.ToArray();
        }

        public void SetOrderArray(Order[] orderArray)
        {
            Orders = orderArray.ToList();
        }

        public override string ToString()
        {
            var orders = string.Format("[{0}]", string.Join(", ", (Orders ?? new List<Order>()).Select(x => string.Format("{0}", x))));
            return string.Format("OrderBatch[count={0}, orders={1}]", BatchCount, orders);
        }
    }
}
