// <copyright file="Component.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// The base class for all nodes in the parser tree.
    /// </summary>
    internal abstract class Component : TreeNode<Component>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Component"/> class.
        /// </summary>
        protected Component()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Component"/> class.
        /// </summary>
        /// <param name="size">the initial child capacity</param>
        protected Component(int size)
            : base(size)
        {
        }

        /// <summary>
        /// Called by a stream to register variables stored in the parsing context.
        /// </summary>
        /// <remarks>
        /// This method should be overridden by subclasses that need to register
        /// one or more parser context variables.
        /// </remarks>
        /// <param name="locals">set of local variables</param>
        public virtual void RegisterLocals(ISet<IParserLocal> locals)
        {
            foreach (Component c in Children)
            {
                c.RegisterLocals(locals);
            }
        }
    }
}
