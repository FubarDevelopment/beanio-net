// <copyright file="ParserComponent.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// Base class for all parser components in that implement <see cref="IParser"/>.
    /// </summary>
    internal abstract class ParserComponent : Component, IParser
    {
        /// <summary>
        /// map key used to store the state of the 'count' attribute
        /// </summary>
        public const string COUNT_KEY = "count";

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserComponent"/> class.
        /// </summary>
        protected ParserComponent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserComponent"/> class.
        /// </summary>
        /// <param name="size">the initial child capacity</param>
        protected ParserComponent(int size)
            : base(size)
        {
        }

        /// <summary>
        /// Gets the size of a single occurrence of this element, which is used to offset
        /// field positions for repeating segments and fields.
        /// </summary>
        /// <remarks>
        /// The concept of size is dependent on the stream format.  The size of an element in a fixed
        /// length stream format is determined by the length of the element in characters, while other
        /// stream formats calculate size based on the number of fields.  Some stream formats,
        /// such as XML, may ignore size settings.
        /// </remarks>
        public abstract int? Size { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this parser or any descendant of this parser is used to identify
        /// a record during unmarshalling.
        /// </summary>
        public abstract bool IsIdentifier { get; set; }

        /// <summary>
        /// Gets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        public abstract bool IsOptional { get; }

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if matched, false otherwise</returns>
        public abstract bool Matches(UnmarshallingContext context);

        /// <summary>
        /// Unmarshals a record
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise</returns>
        public abstract bool Unmarshal(UnmarshallingContext context);

        /// <summary>
        /// Marshals a record
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/></param>
        /// <returns>whether a value was marshalled</returns>
        public abstract bool Marshal(MarshallingContext context);

        /// <summary>
        /// Returns whether this parser or any of its descendant have content for marshalling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>true if there is content for marshalling, false otherwise</returns>
        public abstract bool HasContent(ParsingContext context);

        /// <summary>
        /// Clears the current property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        public abstract void ClearValue(ParsingContext context);

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public abstract void SetValue(ParsingContext context, object value);

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public abstract object GetValue(ParsingContext context);

        /// <summary>
        /// Returns whether a node is a supported child of this node.
        /// </summary>
        /// <remarks>
        /// Called by <see cref="TreeNode{T}.Add"/>.
        /// </remarks>
        /// <param name="child">the node to test</param>
        /// <returns>true if the child is allowed</returns>
        public override bool IsSupportedChild(Component child)
        {
            return child is IParser;
        }
    }
}
