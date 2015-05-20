﻿using System.Reflection;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// The base class for nodes that that make up a stream configuration-groups, records,
    /// segments, fields, constants and wrappers
    /// </summary>
    /// <remarks>
    /// <p>Nodes are organized into a tree structure.</p>
    /// <p>The following attributes apply to XML formatted streams only:</p>
    /// <ul>
    /// <li>xmlName</li>
    /// <li>xmlNamespace</li>
    /// <li>xmlPrefix</li>
    /// </ul>
    /// <p>The following attributes are set during compilation, and are meant for
    /// internal use only:
    /// <ul>
    /// <li>xmlNamespaceAware</li>
    /// </ul>
    /// </p>
    /// </remarks>
    public abstract class ComponentConfig : TreeNode<ComponentConfig>
    {
        /// <summary>
        /// Gets the component type
        /// </summary>
        /// <returns>
        /// One of <see cref="F:ComponentType.Group"/>,
        /// <see cref="F:ComponentType.Record"/>, <see cref="F:ComponentType.Segment"/>
        /// <see cref="F:ComponentType.Field"/>, <see cref="F:ComponentType.Constant"/>,
        /// <see cref="F:ComponentType.Wrapper"/>, or <see cref="F:ComponentType.Stream"/>
        /// </returns>
        public abstract ComponentType ComponentType { get; }

        /// <summary>
        /// Gets or sets the relative position of this component within its parent components
        /// </summary>
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the XML element or attribute name of this component
        /// </summary>
        /// <remarks>
        /// If set to <tt>null</tt> (default), the XML name defaults to the component name
        /// </remarks>
        public string XmlName { get; set; }

        /// <summary>
        /// Gets or sets the XML namespace of this component
        /// </summary>
        /// <remarks>
        /// If set to <tt>null</tt> (default), the namespace is inherited from its parent.
        /// </remarks>
        public string XmlNamespace { get; set; }

        /// <summary>
        /// Gets or sets the XML prefix for the namespace assigned to this component
        /// </summary>
        /// <remarks>
        /// If set to <tt>null</tt> and a namespace is set, the namespace will replace the
        /// default namespace during marshaling.
        /// A prefix should not be set if a namespace is not set.
        /// </remarks>
        public string XmlPrefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this component is namespace aware
        /// </summary>
        public bool IsXmlNamespaceAware { get; set; }
    }
}
