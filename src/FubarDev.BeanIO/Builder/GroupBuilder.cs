// <copyright file="GroupBuilder.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Internal.Config;

namespace BeanIO.Builder
{
    /// <summary>
    /// Builds a new group configuration.
    /// </summary>
    public class GroupBuilder : GroupBuilderSupport<GroupBuilder, GroupConfig>
    {
        private GroupConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupBuilder"/> class.
        /// </summary>
        /// <param name="name">The group name.</param>
        public GroupBuilder(string name)
        {
            _config = new GroupConfig()
                {
                    Name = name,
                };
        }

        /// <summary>
        /// Gets this.
        /// </summary>
        protected override GroupBuilder Me => this;

        /// <summary>
        /// Gets the configuration settings.
        /// </summary>
        protected override GroupConfig Config => _config;

        /// <summary>
        /// Sets the order of this group relative to other children of the same parent.
        /// </summary>
        /// <param name="order">the order.</param>
        /// <returns>The value of <see cref="Me"/>.</returns>
        public GroupBuilder Order(int order)
        {
            Config.Order = order;
            return Me;
        }

        /// <summary>
        /// Builds the group configuration.
        /// </summary>
        /// <returns>The group configuration.</returns>
        public GroupConfig Build()
        {
            return Config;
        }

        /// <summary>
        /// Sets the configuration settings.
        /// </summary>
        /// <param name="config">The configuration settings.</param>
        protected void SetConfig(GroupConfig config)
        {
            _config = config;
        }
    }
}
