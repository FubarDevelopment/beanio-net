// <copyright file="XmlNodeUtil.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using BeanIO.Stream;

using JetBrains.Annotations;

namespace BeanIO.Internal.Parser.Format.Xml
{
    internal static class XmlNodeUtil
    {
        private static readonly XNamespace _xsiNs = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");

        public static XNamespace Xsi => _xsiNs;

        public static string ToConvertedName(this string name, ElementNameConversionMode conversionMode)
        {
            if ((conversionMode & ElementNameConversionMode.RemoveUnderscore) != ElementNameConversionMode.Unchanged)
            {
                name = name.TrimStart('_');
            }

            switch (conversionMode & ElementNameConversionMode.CasingMask)
            {
                case ElementNameConversionMode.AllLower:
                    name = name.ToLowerInvariant();
                    break;
                case ElementNameConversionMode.AllUpper:
                    name = name.ToUpperInvariant();
                    break;
                case ElementNameConversionMode.Decapitalize:
                    name = Introspector.Decapitalize(name);
                    break;
                case ElementNameConversionMode.Capitalize:
                    name = Introspector.Capitalize(name);
                    break;
            }

            return name;
        }

        /// <summary>
        /// Returns all variants for LocalName for a better match between the node name and the <see cref="IXmlNode.LocalName"/>
        /// </summary>
        /// <param name="node">The node to get the alternative names for</param>
        /// <returns>The array of alternative names or <code>null</code> when no <paramref name="node"/> given</returns>
        public static string[] GetNameVariants(this IXmlNode node)
        {
            if (node == null)
                return null;

            var localName = node.LocalName;
            var variants = new List<string>();
            if (localName.StartsWith("_"))
            {
                localName = localName.TrimStart('_');
                variants.Add(localName);
            }

            if (char.IsLower(localName[0]))
            {
                localName = $"{char.ToUpperInvariant(localName[0])}{localName.Substring(1)}";
            }
            else
            {
                localName = $"{char.ToLowerInvariant(localName[0])}{localName.Substring(1)}";
            }

            variants.Add(localName);

            return variants.ToArray();
        }

        /// <summary>
        /// Set the <code>xsi:nil</code> attribute to null
        /// </summary>
        /// <param name="element">The element to set the <code>xsi:nil</code> attribute for</param>
        /// <param name="set">the value to set the attribute to</param>
        public static void SetNil([NotNull] this XElement element, bool set = true)
        {
            element.SetAttributeValue(_xsiNs + "nil", XmlConvert.ToString(set));
        }

        /// <summary>
        /// Creates an <see cref="XName"/> from the <see cref="IXmlNode.Namespace"/> and <see cref="IXmlNode.LocalName"/>.
        /// </summary>
        /// <param name="node">The node to use to create the <see cref="XName"/> from</param>
        /// <param name="ignoreIsNamespaceAware">Should the <see cref="IXmlNode.IsNamespaceAware"/> be ignored?</param>
        /// <returns>the new <see cref="XName"/></returns>
        public static XName ToXName([NotNull] this IXmlNode node, bool ignoreIsNamespaceAware)
        {
            XNamespace ns =
                (string.IsNullOrEmpty(node.Namespace) || (!ignoreIsNamespaceAware && !node.IsNamespaceAware))
                    ? XNamespace.None
                    : XNamespace.Get(node.Namespace);
            var name = ns + node.LocalName;
            return name;
        }

        /// <summary>
        /// Tests if the name of the <paramref name="node"/> equals to <paramref name="name"/>
        /// </summary>
        /// <param name="node">The <see cref="IXmlNode"/> whose name to compare with</param>
        /// <param name="name">The <see cref="XName"/> to compare against</param>
        /// <returns>true, when the name of the <paramref name="node"/> matches the <paramref name="name"/></returns>
        public static bool IsEqual([NotNull] this IXmlNode node, [NotNull] XName name)
        {
            return IsEqual(node, name, null);
        }

        /// <summary>
        /// Tests if the name of the <paramref name="node"/> equals to <paramref name="name"/>
        /// </summary>
        /// <param name="node">The <see cref="IXmlNode"/> whose name to compare with</param>
        /// <param name="name">The <see cref="XName"/> to compare against</param>
        /// <param name="alternativeNodeName">the alternative node name to use instead of <see cref="IXmlNode.LocalName"/></param>
        /// <returns>true, when the name of the <paramref name="node"/> matches the <paramref name="name"/></returns>
        public static bool IsEqual([NotNull] this IXmlNode node, [NotNull] XName name, string alternativeNodeName)
        {
            if ((alternativeNodeName ?? node.LocalName) != name.LocalName)
                return false;
            if (!node.IsNamespaceAware)
                return true;
            return string.Equals(name.NamespaceName, node.Namespace, StringComparison.Ordinal);
        }

        /// <summary>
        /// Tests if the name of the <paramref name="node"/> equals to <paramref name="name"/>
        /// </summary>
        /// <param name="alternativeNodeNames">the alternative node name to use instead of <see cref="IXmlNode.LocalName"/>, when the default name cannot be found</param>
        /// <param name="node">The <see cref="IXmlNode"/> whose name to compare with</param>
        /// <param name="name">The <see cref="XName"/> to compare against</param>
        /// <returns>true, when the name of the <paramref name="node"/> matches the <paramref name="name"/></returns>
        public static bool IsEqual([CanBeNull] this IEnumerable<string> alternativeNodeNames, [NotNull] IXmlNode node, [NotNull] XName name)
        {
            if (node.IsEqual(name))
                return true;
            if (alternativeNodeNames == null)
                return false;
            return alternativeNodeNames.Any(alternativeNodeName => node.IsEqual(name, alternativeNodeName));
        }

        /// <summary>
        /// Tests if an element is nil.
        /// </summary>
        /// <param name="element">the element to test</param>
        /// <returns><code>true</code> if the element is nil</returns>
        public static bool IsNil(this XElement element)
        {
            var nil = element.Attributes(_xsiNs + "nil").Select(x => x.Value).SingleOrDefault();
            return !string.IsNullOrEmpty(nil) && XmlConvert.ToBoolean(nil);
        }

        /// <summary>
        /// Returns the value of an attribute for an element.
        /// </summary>
        /// <param name="element">the element to check</param>
        /// <param name="definition">the definition of the attribute to retrieve from the element</param>
        /// <param name="alternativeNames">the alternative names to be used instead of <see cref="IXmlNode.LocalName"/></param>
        /// <returns>the defined attribute value, or <code>null</code> if the attribute was not found on the element</returns>
        public static string GetAttribute([CanBeNull] this XElement element, [NotNull] IXmlNode definition, params string[] alternativeNames)
        {
            var attribute = element?.Attributes().SingleOrDefault(x => alternativeNames.IsEqual(definition, x.Name));
            return attribute?.Value;
        }

        /// <summary>
        /// Returns the child text from a DOM node.
        /// </summary>
        /// <param name="node">the node to parse</param>
        /// <returns>the node text, or <code>null</code> if the node did not contain any text</returns>
        public static string GetText([NotNull] this XContainer node)
        {
            StringBuilder s = null;
            foreach (var child in node.Nodes())
            {
                switch (child.NodeType)
                {
                    case XmlNodeType.Text:
                        if (s == null)
                            s = new StringBuilder();
                        s.Append(((XText)child).Value);
                        break;
                    case XmlNodeType.CDATA:
                        if (s == null)
                            s = new StringBuilder();
                        s.Append(((XCData)child).Value);
                        break;
                }
            }

            return s?.ToString();
        }

        /// <summary>
        /// Returns a sibling element that matches a given definition, or <code>null</code> if
        /// no match is found.
        /// </summary>
        /// <param name="sibling">the sibling DOM element to begin the search</param>
        /// <param name="target">the node to search for</param>
        /// <param name="alternativeTargetNames">the alternative target names to be used instead of <see cref="IXmlNode.LocalName"/></param>
        /// <returns>the matching element, or <code>null</code> if not found</returns>
        public static XElement FindSibling(XElement sibling, IXmlNode target, params string[] alternativeTargetNames)
        {
            var element = sibling?.ElementsAfterSelf()
                                 .FirstOrDefault(x => alternativeTargetNames.IsEqual(target, x.Name));
            return element;
        }

        /// <summary>
        /// Finds the Nth matching child of a DOM element.
        /// </summary>
        /// <param name="parent">the parent DOM node</param>
        /// <param name="target">the node to search for</param>
        /// <param name="offset">the occurrence of the matching node</param>
        /// <param name="alternativeTargetNames">the alternative target names to be used instead of <see cref="IXmlNode.LocalName"/></param>
        /// <returns>the matching element, or <code>null</code> if no match is found</returns>
        public static XElement FindChild(XNode parent, IXmlNode target, int offset, params string[] alternativeTargetNames)
        {
            var container = parent as XContainer;
            var element = container?.Elements().Where(x => alternativeTargetNames.IsEqual(target, x.Name))
                                   .Skip(offset)
                                   .FirstOrDefault();
            return element;
        }
    }
}
