﻿using System;
using System.Collections.Generic;
using System.Text;

using JetBrains.Annotations;

namespace BeanIO.Stream.Xml
{
    /// <summary>
    /// Stores configuration settings for parsing XML formatted streams
    /// </summary>
    /// <remarks>
    /// By default, indentation is disabled and an XML header will be written to an output stream
    /// </remarks>
    public class XmlParserConfiguration
    {
        private readonly Dictionary<string, string> _namespaceMap = new Dictionary<string, string>();

        /// <summary>
        /// the number of spaces to indent each level of XML, or <code>null</code>
        /// if indentation is disabled.
        /// </summary>
        public int? Indentation { get; set; }

        /// <summary>
        /// Gets a value indicating whether XML output will be indented
        /// </summary>
        public bool IsIndentionEnabled
        {
            get { return Indentation != null && Indentation >= 0; }
        }

        /// <summary>
        /// Gets or sets the text used to terminate a line when indentation is enabled
        /// </summary>
        /// <remarks>
        /// When set to <code>null</code> (the default), the line separator is set to the
        /// value of the <see cref="Environment.NewLine"/> system property.
        /// </remarks>
        public string LineSeparator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the XML header is suppressed
        /// </summary>
        public bool SuppressHeader { get; set; }

        /// <summary>
        /// Gets or sets the XML version to include in the document header
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the XML character encoding to include in the document header
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets a map of namespace URI's to prefixes to be set on the root element
        /// </summary>
        public IDictionary<string, string> NamespaceMap
        {
            get { return _namespaceMap; }
        }

        /// <summary>
        /// Adds a namespace to be set on the root element
        /// </summary>
        /// <param name="prefix">the namespace prefix</param>
        /// <param name="uri">the namespace URI</param>
        public void AddNamespace([NotNull] string prefix, [NotNull] string uri)
        {
            if (prefix == null)
                throw new ArgumentNullException("prefix");
            if (uri == null)
                throw new ArgumentNullException("uri");
            _namespaceMap[uri] = prefix;
        }

        /// <summary>
        /// Sets the list of namespaces to be set on the root element
        /// </summary>
        /// <remarks>The list should be formatted as a space delimited list of alternating prefixes and uri's</remarks>
        /// <example>
        /// xsd http://www.w3.org/2001/XMLSchema b http://www.beanio.org/2011/01
        /// </example>
        /// <param name="list">the space delimited list of namespaces</param>
        public void SetNamespaces(string list)
        {
            _namespaceMap.Clear();

            if (string.IsNullOrWhiteSpace(list))
                return;

            var s = list.Trim().Replace('\t', ' ').Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if ((s.Length & 1)==1)
                throw new BeanIOConfigurationException("Invalid namespaces setting.  Must follow 'prefix uri prefix uri' pattern.");

            for (int i = 0; i != s.Length; i += 2)
                AddNamespace(s[i + 1], s[i]);
        }
    }
}