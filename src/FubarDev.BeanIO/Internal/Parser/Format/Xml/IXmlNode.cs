// <copyright file="IXmlNode.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Xml;

namespace BeanIO.Internal.Parser.Format.Xml
{
    /// <summary>
    /// An XML node
    /// </summary>
    internal interface IXmlNode
    {
        /// <summary>
        /// Gets the XML node type
        /// </summary>
        XmlNodeType Type { get; }

        /// <summary>
        /// Gets the XML local name for this node.
        /// </summary>
        string LocalName { get; }

        /// <summary>
        /// Gets the namespace of this node.  If there is no namespace for this
        /// node, or this node is not namespace aware, <code>null</code> is returned.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets a value indicating whether a namespace was configured for this node,
        /// and is therefore used to unmarshal and marshal the node.
        /// </summary>
        bool IsNamespaceAware { get; }

        /// <summary>
        /// Gets the namespace prefix for marshaling this node, or <code>null</code>
        /// if the namespace should override the default namespace.
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// Gets a value indicating whether this node is nillable.
        /// </summary>
        bool IsNillable { get; }

        /// <summary>
        /// Gets a value indicating whether this node may repeat in the context of its immediate parent.
        /// </summary>
        bool IsRepeating { get; }
    }
}
