// <copyright file="RecordBuilder.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Internal.Config;
using BeanIO.Internal.Util;

namespace BeanIO.Builder
{
    /// <summary>
    /// Builds a new record configuration
    /// </summary>
    public class RecordBuilder : SegmentBuilderSupport<RecordBuilder, RecordConfig>
    {
        private RecordConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordBuilder"/> class.
        /// </summary>
        /// <param name="name">The segment name</param>
        public RecordBuilder(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordBuilder"/> class.
        /// </summary>
        /// <param name="name">The segment name</param>
        /// <param name="type">The record type</param>
        public RecordBuilder(string name, Type type)
        {
            _config = new RecordConfig()
            {
                Name = name,
                Type = type.GetAssemblyQualifiedName(),
            };
        }

        /// <summary>
        /// Gets this.
        /// </summary>
        protected override RecordBuilder Me => this;

        /// <summary>
        /// Gets the configuration settings.
        /// </summary>
        protected override RecordConfig Config => _config;

        /// <summary>
        /// Sets the order of this record relative to other children of the same parent.
        /// </summary>
        /// <param name="order">the order (starting with 1)</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public RecordBuilder Order(int order)
        {
            Config.Order = order;
            return Me;
        }

        /// <summary>
        /// Sets the minimum length of the record (i.e the number of fields
        /// in a delimited record, or the number of characters in a fixed length
        /// record).
        /// </summary>
        /// <param name="min">the minimum length</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public RecordBuilder MinLength(int min)
        {
            Config.MinLength = min;
            return Me;
        }

        /// <summary>
        /// Sets the maximum length of the record (i.e the number of fields
        /// in a delimited record, or the number of characters in a fixed length
        /// record).
        /// </summary>
        /// <param name="max">the maximum length, or -1 if unbounded</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public RecordBuilder MaxLength(int max)
        {
            Config.MaxLength = max < 0 ? int.MaxValue : max;
            return Me;
        }

        /// <summary>
        /// Sets the length of the record (i.e the number of fields in a delimited
        /// record, or the number of characters in a fixed length record).
        /// </summary>
        /// <param name="n">the length</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public RecordBuilder Length(int n)
        {
            return Length(n, n);
        }

        /// <summary>
        /// Sets the minimum and maximum length of the record (i.e the number of fields
        /// in a delimited record, or the number of characters in a fixed length
        /// record).
        /// </summary>
        /// <param name="min">the minimum length</param>
        /// <param name="max">the maximum length, or -1 if unbounded</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public RecordBuilder Length(int min, int max)
        {
            return MinLength(min).MaxLength(max);
        }

        /// <summary>
        /// Sets the length of the record for identification
        /// </summary>
        /// <param name="n">the length</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public RecordBuilder RidLength(int n)
        {
            return RidLength(n, n);
        }

        /// <summary>
        /// Sets the minimum and maximum length of the record for identification.
        /// </summary>
        /// <param name="min">the minimum length</param>
        /// <param name="max">the maximum length, or -1 if unbounded</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public RecordBuilder RidLength(int min, int max)
        {
            Config.MinMatchLength = min;
            Config.MaxMatchLength = max < 0 ? int.MaxValue : max;
            return Me;
        }

        /// <summary>
        /// Builds the record configuration
        /// </summary>
        /// <returns>the record configuration</returns>
        public RecordConfig Build()
        {
            return Config;
        }

        /// <summary>
        /// Sets the configuration settings
        /// </summary>
        /// <param name="config">The configuration settings</param>
        protected void SetConfig(RecordConfig config)
        {
            _config = config;
        }
    }
}
