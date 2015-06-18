using System;
using System.Collections.Generic;
using System.Text;

using JetBrains.Annotations;

namespace BeanIO.Stream.Xml
{
    /// <summary>
    /// An <see cref="ElementStack"/> is used internally by <see cref="Stream.Xml.XmlWriter"/> for managing
    /// namespace declarations as an XML document is written.
    /// </summary>
    internal class ElementStack
    {
        public ElementStack(ElementStack parent, string ns, string prefix, string name)
        {
            Parent = parent;
            Namespace = ns;
            Prefix = prefix;
            Name = name;

            if (prefix == null)
            {
                DefaultNamespace = ns ?? string.Empty;
            }
            else
            {
                DefaultNamespace = parent == null ? string.Empty : parent.DefaultNamespace;
                AddNamespace(prefix, DefaultNamespace);
            }
        }

        /// <summary>
        /// Gets the parent element in this stack
        /// </summary>
        public ElementStack Parent { get; private set; }

        /// <summary>
        /// Gets the XML namespace of this element
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Gets the XML namespace prefix of this element, or <code>null</code>
        /// if no prefix was assigned
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Gets the XML element name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the default XML namespace for a child element
        /// </summary>
        public string DefaultNamespace { get; private set; }

        /// <summary>
        /// Gets or sets the XML namespaces declared by this element
        /// </summary>
        [CanBeNull]
        public Dictionary<string, string> Namespaces { get; set; }

        /// <summary>
        /// Creates a new <see cref="ElementStack"/> from its token value.
        /// </summary>
        /// <param name="parent">the parent stack element</param>
        /// <param name="token">the element token</param>
        /// <returns>the new <see cref="ElementStack"/></returns>
        public static ElementStack FromToken([CanBeNull] ElementStack parent, [NotNull] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException("token");

            string ns = null;
            string prefix = null;

            int start = 0;
            int pos;

            // parse out the namespace
            if (token.StartsWith("{"))
            {
                pos = token.IndexOf('}');
                ns = token.Substring(1, pos);
                start = pos + 1;
            }

            // check for a prefix
            pos = token.IndexOf(':', start);
            if (pos > 0)
            {
                prefix = token.Substring(start, pos);
                start = pos + 1;
            }

            var name = token.Substring(start);

            return new ElementStack(parent, ns, prefix, name);
        }

        /// <summary>
        /// Returns whether this element uses the default XML namespace
        /// </summary>
        /// <returns>true if this element uses the default XML namespace</returns>
        public bool IsDefaultNamespace()
        {
            if (Namespace == null)
                return true;
            if (Parent == null)
                return Namespace.Length == 0;
            return string.Equals(Namespace, DefaultNamespace, StringComparison.Ordinal);
        }

        /// <summary>
        /// Tests whether a given namespace matches the current default namespace
        /// </summary>
        /// <param name="ns">the XML namespace to test</param>
        /// <returns>true if a default namespace is assigned and the given namespace matches it</returns>
        public bool IsDefaultNamespace(string ns)
        {
            return DefaultNamespace != null && string.Equals(DefaultNamespace, ns, StringComparison.Ordinal);
        }

        /// <summary>
        /// Adds an XML namespace declaration for this element
        /// </summary>
        /// <param name="prefix">the namespace prefix</param>
        /// <param name="ns">the namespace</param>
        public void AddNamespace(string prefix, string ns)
        {
            if (Namespaces == null)
                Namespaces = new Dictionary<string, string>(StringComparer.Ordinal);
            Namespaces[ns] = prefix;
        }

        /// <summary>
        /// Searches this element and its ancestors for a prefix declared for a
        /// given XML namespace.
        /// </summary>
        /// <param name="ns">the namespace to search for</param>
        /// <returns>the previously declared prefix or <code>null</code> if no prefix has been declared</returns>
        public string FindPrefix(string ns)
        {
            if (Namespaces != null)
            {
                string prefix;
                if (Namespaces.TryGetValue(ns, out prefix))
                    return prefix;
            }
            if (Parent != null)
                return Parent.FindPrefix(ns);
            return null;
        }

        /// <summary>
        /// Creates a token for this stack element containing the namespace prefix and element name.
        /// </summary>
        /// <returns>a token representing this stack element</returns>
        public string ToToken()
        {
            var s = new StringBuilder();
            if (Namespace != null)
                s.Append("{").Append(Namespace).Append("}");
            if (Prefix != null)
                s.Append(Prefix).Append(':');
            s.Append(Name);
            return s.ToString();
        }

        public override string ToString()
        {
            return ToToken();
        }
    }
}
