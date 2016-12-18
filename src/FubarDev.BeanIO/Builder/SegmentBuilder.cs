// <copyright file="SegmentBuilder.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Internal.Config;

namespace BeanIO.Builder
{
    /// <summary>
    /// Builds a new segment configuration
    /// </summary>
    public class SegmentBuilder : SegmentBuilderSupport<SegmentBuilder, SegmentConfig>
    {
        private SegmentConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentBuilder"/> class.
        /// </summary>
        /// <param name="name">The segment name</param>
        public SegmentBuilder(string name)
        {
            _config = new SegmentConfig()
                {
                    Name = name,
                };
        }

        /// <summary>
        /// Gets the configuration settings.
        /// </summary>
        protected internal override SegmentConfig Config => _config;

        /// <summary>
        /// Gets this.
        /// </summary>
        protected override SegmentBuilder Me => this;

        /// <summary>
        /// Indicates the number of occurrences of this segment is governed by another field.
        /// </summary>
        /// <param name="reference">The name of the field that governs the occurrences of this segment</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public SegmentBuilder OccursRef(string reference)
        {
            Config.OccursRef = reference;
            return Me;
        }

        /// <summary>
        /// Indicates the XML element is nillable.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public SegmentBuilder IsNillable()
        {
            Config.IsNillable = true;
            return Me;
        }

        /// <summary>
        /// Sets the position of the field.
        /// </summary>
        /// <param name="position">the position</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public SegmentBuilder At(int position)
        {
            Config.Position = position;
            return Me;
        }

        /// <summary>
        /// Builds the segment configuration.
        /// </summary>
        /// <returns>The segment configuration</returns>
        public SegmentConfig Build()
        {
            return Config;
        }

        /// <summary>
        /// Sets the configuration settings
        /// </summary>
        /// <param name="config">The configuration settings</param>
        protected void SetConfig(SegmentConfig config)
        {
            _config = config;
        }
    }
}
