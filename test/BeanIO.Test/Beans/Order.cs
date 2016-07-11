// <copyright file="Order.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace BeanIO.Beans
{
    public class Order
    {
        public string Id { get; set; }

        public DateTime Date { get; set; }

        public string PaymentMethod { get; set; }

        public Person Customer { get; set; }

        public Person Shipper { get; set; }

        public List<OrderItem> Items { get; set; }

        public Dictionary<string, OrderItem> ItemMap { get; set; }

        public override string ToString()
        {
            return Id;
        }
    }
}
