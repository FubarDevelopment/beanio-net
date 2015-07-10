using System.Collections.Generic;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// A group is used to organize records and the overall structure of a stream.
    /// </summary>
    /// <remarks>
    /// <para>A group may contain records and/or other groups.
    /// In many cases, a group does not map to any physical aspect of a stream.</para>
    /// <para>The <see cref="P:GroupConfig.MinSize"/>, <see cref="P:GroupConfig.MaxSize"/>,
    /// and <see cref="P:GroupConfig.IsNillable"/> attributes do not apply to groups.</para>
    /// </remarks>
    public class GroupConfig : PropertyConfig, ISelectorConfig
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
        public override ComponentType ComponentType
        {
            get { return ComponentType.Group; }
        }

        /// <summary>
        /// Gets the name of the property descendant to use for the Map key when
        /// <see cref="PropertyConfig.Collection"/> is set to <see cref="Dictionary{TKey,TValue}"/> (map).
        /// </summary>
        public override string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets or sets the order of this component within the context of its parent group.
        /// </summary>
        /// <remarks>
        /// Records and groups assigned the same order number may appear in any order.
        /// </remarks>
        /// <returns>the component order (starting at 1)</returns>
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets the name of the target property for this group, which can be used
        /// in lieu of <see cref="P:GroupConfig.Type"/> to return descendant property types.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Returns whether a node is a supported child of this node.
        /// </summary>
        /// <remarks>
        /// Called by <see cref="TreeNode{T}.Add"/>.
        /// </remarks>
        /// <param name="child">the node to test</param>
        /// <returns>true if the child is allowed</returns>
        public override bool IsSupportedChild(ComponentConfig child)
        {
            switch (child.ComponentType)
            {
                case ComponentType.Group:
                case ComponentType.Record:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Sets the name of the property descendant to use for the
        /// Map key when <see cref="P:GroupConfig.Collection"/> is set to
        /// <see cref="Dictionary{TKey,TValue}"/> (map).
        /// </summary>
        /// <param name="key">The key property name</param>
        public void SetKey(string key)
        {
            _key = key;
        }
    }
}
