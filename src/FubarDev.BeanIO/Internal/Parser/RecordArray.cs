// <copyright file="RecordArray.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="Parser"/> tree component for parsing an array of bean objects, where
    /// a bean object is mapped to a <see cref="Record"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="RecordArray"/> supports a single <see cref="Record"/> child.
    /// </remarks>
    internal class RecordArray : RecordCollection
    {
        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public override PropertyType Type => Internal.Parser.PropertyType.AggregationArray;

        /// <summary>
        /// Gets or sets the class type of the array
        /// </summary>
        public Type ElementType { get; set; }

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public override object GetValue(ParsingContext context)
        {
            var collection = GetCollection(context);
            if (collection == null)
                return null;

            try
            {
                var index = 0;
                var array = Array.CreateInstance(ElementType, collection.Count);
                foreach (var obj in collection)
                    array.SetValue(obj, index++);
                return array;
            }
            catch (Exception ex)
            {
                throw new BeanReaderException("Failed to set array value.", ex);
            }
        }

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public override void SetValue(ParsingContext context, object value)
        {
            ICollection collection = null;
            if (value != null)
            {
                var array = (Array)value;
                var length = array.Length;
                if (length > 0)
                {
                    var list = new List<object>(length);
                    collection = list;
                    for (var i = 0; i != length; ++i)
                        list.Add(array.GetValue(i));
                }
            }

            base.SetValue(context, collection);
        }

        protected override object CreateAggregationType()
        {
            return new List<object>();
        }
    }
}
