// <copyright file="XmlParserConfiguration.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BeanIO.Stream.Xml
{
    /// <summary>
    /// Stores configuration settings for parsing XML formatted streams.
    /// </summary>
    /// <remarks>
    /// By default, indentation is disabled and an XML header will be written to an output stream.
    /// </remarks>
    public class XmlParserConfiguration
    {
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false);

        private readonly Dictionary<string, string> _namespaceMap = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlParserConfiguration"/> class.
        /// </summary>
        public XmlParserConfiguration()
        {
            Version = new Version(1, 0);
            Encoding = _defaultEncoding.WebName;
        }

        /// <summary>
        /// Gets or sets the number of spaces to indent each level of XML, or <see langword="null" />
        /// if indentation is disabled.
        /// </summary>
        public int? Indentation { get; set; }

        /// <summary>
        /// Gets a value indicating whether XML output will be indented.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Indentation))]
        public bool IsIndentionEnabled => Indentation != null && Indentation >= 0;

        /// <summary>
        /// Gets or sets the text used to terminate a line when indentation is enabled.
        /// </summary>
        /// <remarks>
        /// When set to <see langword="null" /> (the default), the line separator is set to the
        /// value of the <see cref="System.IO.TextWriter.NewLine"/> system property.
        /// </remarks>
        public string? LineSeparator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the XML header is suppressed.
        /// </summary>
        public bool SuppressHeader { get; set; }

        /// <summary>
        /// Gets or sets the XML version to include in the document header.
        /// </summary>
        public Version? Version { get; set; }

        /// <summary>
        /// Gets or sets the XML character encoding to include in the document header.
        /// </summary>
        public string Encoding { get; set; }

        /// <summary>
        /// Gets a map of namespace URI's to prefixes to be set on the root element.
        /// </summary>
        public IDictionary<string, string> NamespaceMap => _namespaceMap;

        /// <summary>
        /// Adds a namespace to be set on the root element.
        /// </summary>
        /// <param name="prefix">the namespace prefix.</param>
        /// <param name="uri">the namespace URI.</param>
        public void AddNamespace(string prefix, string uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            _namespaceMap[uri] = prefix ?? throw new ArgumentNullException(nameof(prefix));
        }

        /// <summary>
        /// Sets the list of namespaces to be set on the root element.
        /// </summary>
        /// <remarks>The list should be formatted as a space delimited list of alternating prefixes and uri's.</remarks>
        /// <example>
        /// <code>xsd http://www.w3.org/2001/XMLSchema b http://www.beanio.org/2011/01 </code>
        /// </example>
        /// <param name="list">the space delimited list of namespaces.</param>
        public void SetNamespaces(string list)
        {
            _namespaceMap.Clear();

            if (string.IsNullOrWhiteSpace(list))
                return;

            var s = list.Trim().Replace('\t', ' ').Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if ((s.Length & 1) == 1)
                throw new BeanIOConfigurationException("Invalid namespaces setting.  Must follow 'prefix uri prefix uri' pattern.");

            for (var i = 0; i != s.Length; i += 2)
                AddNamespace(s[i + 1], s[i]);
        }

        /// <summary>
        /// Gets the XML encoding.
        /// </summary>
        /// <returns>The <see cref="Encoding"/> to be used for XML reading/writing.</returns>
        public Encoding? GetEncoding()
        {
            if (string.IsNullOrEmpty(Encoding))
                return null;
            var result = System.Text.Encoding.GetEncoding(Encoding);
            if (result.WebName == "utf-8")
                return _defaultEncoding;
            return result;
        }
    }
}
