// <copyright file="IIteration.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// Repeating components must implement <see cref="IIteration"/> to offset record positions
    /// during marshalling and unmarshalling.
    /// </summary>
    internal interface IIteration
    {
        /// <summary>
        /// Gets the size of the components that make up a single iteration.
        /// </summary>
        int IterationSize { get; }

        /// <summary>
        /// Gets a value indicating whether the iteration size is variable based on another field in the record.
        /// </summary>
        bool IsDynamicIteration { get; }

        /// <summary>
        /// Returns the index of the current iteration relative to its parent.
        /// </summary>
        /// <param name="context">The context of this iteration.</param>
        /// <returns>the index of the current iteration.</returns>
        int GetIterationIndex(ParsingContext context);
    }
}
