// <copyright file="SegmentConfig.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// A segment is used to combine fields, constants and other segments. A Wrapper component may also be added to segment.
    /// </summary>
    /// <remarks>
    /// <para>A segment can be bound to a bean object by setting <see cref="P:SegmentConfig.Type"/>.
    /// (The <see cref="P:SegmentConfig.IsBound"/> attribute is ignored for segments- setting <see cref="P:SegmentConfig.Type"/>
    /// to null has the same effect as setting <see cref="P:SegmentConfig.IsBound"/> to false.)</para>
    /// <para>A segment may repeat if its maximum occurrences is greater than one, and be
    /// bound to a collection or array by setting <see cref="P:SegmentConfig.Collection"/>.</para>
    /// <para>Segments will have their position calculated automatically during compilation.</para>
    /// <para>The <see cref="SegmentConfig.IsConstant"/> attribute is set during compilation, and is meant for
    /// internal use only.</para>
    /// </remarks>
    public class SegmentConfig : PropertyConfig
    {
        private string _key;

        /// <summary>
        /// Gets the component type
        /// </summary>
        /// <returns>
        /// One of <see cref="F:ComponentType.Group"/>,
        /// <see cref="F:ComponentType.Record"/>, <see cref="F:ComponentType.Segment"/>
        /// <see cref="F:ComponentType.Field"/>, <see cref="F:ComponentType.Constant"/>,
        /// <see cref="F:ComponentType.Wrapper"/>, or <see cref="F:ComponentType.Stream"/>
        /// </returns>
        public override ComponentType ComponentType => ComponentType.Segment;

        /// <summary>
        /// Gets a list of all immediate children including segments, fields and
        /// constants and the immediate children of any wrapper child.
        /// </summary>
        public IEnumerable<PropertyConfig> PropertyList => GetPropertyList(this);

        /// <summary>
        /// Gets or sets a value indicating whether this segment is used to define a bean constant.
        /// </summary>
        public bool IsConstant { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the default existence setting for this
        /// segment as calculated during pre-processing.
        /// </summary>
        public bool IsDefaultExistence { get; set; }

        /// <summary>
        /// Gets or sets the name of the target property for this segment.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Gets the name of the property descendant to use for the Map key when
        /// <see cref="PropertyConfig.Collection"/> is set to <see cref="Dictionary{TKey,TValue}"/> (map).
        /// </summary>
        public override string Key => _key;

        /// <summary>
        /// Sets the name of the property descendant to use for the
        /// Map key when <see cref="P:SegmentConfig.Collection"/> is set to
        /// <see cref="Dictionary{TKey,TValue}"/> (map).
        /// </summary>
        /// <param name="key">The key property name</param>
        public void SetKey(string key)
        {
            _key = key;
        }

        /// <summary>
        /// Returns whether a node is a supported child of this node.
        /// </summary>
        /// <remarks>
        /// Called by <see cref="Util.TreeNode{T}.Add"/>.
        /// </remarks>
        /// <param name="child">the node to test</param>
        /// <returns>true if the child is allowed</returns>
        public override bool IsSupportedChild(ComponentConfig child)
        {
            switch (child.ComponentType)
            {
                case ComponentType.Segment:
                case ComponentType.Field:
                case ComponentType.Constant:
                case ComponentType.Wrapper:
                    return true;
                default:
                    return false;
            }
        }

        private static IEnumerable<PropertyConfig> GetPropertyList(ComponentConfig parent)
        {
            foreach (var child in parent.Children)
            {
                switch (child.ComponentType)
                {
                    case ComponentType.Segment:
                    case ComponentType.Field:
                    case ComponentType.Constant:
                        yield return (PropertyConfig)child;
                        break;
                    case ComponentType.Wrapper:
                        foreach (var propertyOfChild in GetPropertyList(child))
                            yield return propertyOfChild;
                        break;
                }
            }
        }
    }
}
