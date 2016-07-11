// <copyright file="ArrayParser.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// The <see cref="ArrayParser"/> class
    /// </summary>
    internal class ArrayParser : CollectionParser
    {
        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public override PropertyType Type => Internal.Parser.PropertyType.AggregationArray;

        /// <summary>
        /// Gets a value indicating whether this aggregation is a property of its parent bean object.
        /// </summary>
        public override bool IsProperty => true;

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public override object GetValue(ParsingContext context)
        {
            if (IsInvalid(context))
                return base.GetValue(context);

            var collection = GetCollection(context);
            if (collection == null)
                return null;

            var index = 0;
            var array = Array.CreateInstance(ElementType, collection.Count);
            foreach (var obj in collection)
            {
                if (obj != null)
                    array.SetValue(obj, index);
                ++index;
            }

            return array;
        }

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public override void SetValue(ParsingContext context, object value)
        {
            IList collection = null;
            if (value != null)
            {
                var arr = (Array)value;
                var length = arr.Length;
                if (length > 0)
                {
                    collection = (IList)(from constructor in typeof(List<>).MakeGenericType(ElementType).GetTypeInfo().DeclaredConstructors
                                         let parameters = constructor.GetParameters()
                                         where parameters.Length == 1 && parameters[0].ParameterType == typeof(int)
                                         select constructor).Single().Invoke(new object[] { length });
                    for (int i = 0; i != length; ++i)
                        collection.Add(arr.GetValue(i));
                }
            }

            base.SetValue(context, collection);
        }

        protected override IList CreateCollection()
        {
            var newList = typeof(List<>).MakeGenericType(ElementType).NewInstance();
            return (IList)newList;
        }
    }
}
