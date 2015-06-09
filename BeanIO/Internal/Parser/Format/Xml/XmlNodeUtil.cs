using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using JetBrains.Annotations;

namespace BeanIO.Internal.Parser.Format.Xml
{
    public static class XmlNodeUtil
    {
        private static readonly XNamespace _xsiNs = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");

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
            if (node.LocalName != name.LocalName)
                return false;
            if (!node.IsNamespaceAware)
                return true;
            return string.Equals(name.NamespaceName, node.Namespace, StringComparison.Ordinal);
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
        /// <returns>the defined attribute value, or <code>null</code> if the attribute was not found on the element</returns>
        public static string GetAttribute([CanBeNull] this XElement element, [NotNull] IXmlNode definition)
        {
            if (element == null)
                return null;

            var attribute = element.Attributes().SingleOrDefault(x => definition.IsEqual(x.Name));
            if (attribute == null)
                return null;
            return attribute.Value;
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
            return s == null ? null : s.ToString();
        }

        /// <summary>
        /// Returns a sibling element that matches a given definition, or <code>null</code> if
        /// no match is found.
        /// </summary>
        /// <param name="sibling">the sibling DOM element to begin the search</param>
        /// <param name="target">the node to search for</param>
        /// <returns>the matching element, or <code>null</code> if not found</returns>
        public static XElement FindSibling(XElement sibling, IXmlNode target)
        {
            if (sibling == null)
                return null;

            var element = sibling.ElementsAfterSelf().FirstOrDefault(x => target.IsEqual(x.Name));
            return element;
        }

        /// <summary>
        /// Finds the Nth matching child of a DOM element.
        /// </summary>
        /// <param name="parent">the parent DOM node</param>
        /// <param name="target">the node to search for</param>
        /// <param name="offset">the occurrence of the matching node</param>
        /// <returns>the matching element, or <code>null</code> if no match is found</returns>
        public static XElement FindChild(XNode parent, IXmlNode target, int offset)
        {
            var container = parent as XContainer;
            if (container == null)
                return null;
            var element = container
                .Elements().Where(x => target.IsEqual(x.Name))
                .Skip(offset)
                .FirstOrDefault();
            return element;
        }
    }
}
