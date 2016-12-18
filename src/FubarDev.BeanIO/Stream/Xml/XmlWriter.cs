// <copyright file="XmlWriter.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using BeanIO.Internal.Parser.Format.Xml;
using BeanIO.Internal.Parser.Format.Xml.Annotations;
using BeanIO.Internal.Util;

using JetBrains.Annotations;

namespace BeanIO.Stream.Xml
{
    /// <summary>
    /// A <see cref="XmlWriter"/> is used to write records to a XML output stream
    /// </summary>
    /// <remarks>
    /// <para>A document object model (DOM) is used to represent a record.  Group elements,
    /// as indicated by a user data key (see below), are not closed when a record is written.
    /// When <see cref="Write(object)"/> with <code>null</code> is called, an open group element is closed.
    /// Finally, calling <see cref="Flush"/> will close all remaining group elements and complete the document.</para>
    /// <para>A <see cref="XmlWriter"/> makes use of the DOM user data feature to pass additional
    /// information to and from the parser.  The <see cref="IsGroupElementAnnotation"/> user data is
    /// a <see cref="bool"/> value added to an element to indicate the element is group.
    /// And the <see cref="NamespaceModeAnnotation"/> with <see cref="NamespaceHandlingMode.IgnoreNamespace"/> set on
    /// elements where the XML namespace should be ignored when writing to the output stream.</para>
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1124:DoNotUseRegions", Justification = "Reviewed.")]
    public class XmlWriter : IRecordWriter, IStatefulWriter
    {
        private static readonly bool DELTA_ENABLED = Settings.Instance.GetBoolean(Settings.XML_WRITER_UPDATE_STATE_USING_DELTA);

        #region map keys for storing state information for implementing StatefulWriter
        private static readonly string OUTPUT_HEADER_KEY = "header";
        private static readonly string NAMESPACE_MAP_KEY = "nsMap";
        private static readonly string LEVEL_KEY = "level";
        private static readonly string STACK_ELEMENT_KEY = "xml";
        private static readonly string STACK_NS_MAP_KEY = "nsMap";
        #endregion

        /// <summary>
        /// The underlying writer
        /// </summary>
        private readonly FilterWriter _writer;

        /// <summary>
        /// XML parser configuration
        /// </summary>
        private readonly XmlParserConfiguration _config;

        /// <summary>
        /// The XML stream writer to write to
        /// </summary>
        private readonly System.Xml.XmlWriter _out;

        private readonly Encoding _encoding;

        /// <summary>
        /// Map of auto-generated namespaces to namespace prefixes
        /// </summary>
        private Dictionary<string, string> _namespaceMap = new Dictionary<string, string>();

        private ISet<string> _usedPrefixes = new HashSet<string>();

        private ElementStack _elementStack;

        private int _namespaceIndex;

        /// <summary>
        /// whether a XML header needs to be output before writing a record
        /// </summary>
        private bool _outputHeader;

        /// <summary>
        /// the minimum level last stored when the state was updated
        /// </summary>
        private int _dirtyLevel;

        private int _level;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlWriter"/> class.
        /// </summary>
        /// <param name="writer">the output stream to write to</param>
        public XmlWriter([NotNull] TextWriter writer)
            : this(writer, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlWriter"/> class.
        /// </summary>
        /// <param name="writer">the output stream to write to</param>
        /// <param name="config">the XML writer configuration</param>
        public XmlWriter([NotNull] TextWriter writer, XmlParserConfiguration config)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            _config = config ?? new XmlParserConfiguration();
            _encoding = _config.GetEncoding();
            _writer = new FilterWriter(writer);

            var writerSettings = new XmlWriterSettings()
            {
                Indent = _config.IsIndentionEnabled,
                IndentChars = new string(' ', _config.Indentation.GetValueOrDefault()),
                OmitXmlDeclaration = _config.SuppressHeader,
                NewLineChars = _config.LineSeparator ?? writer.NewLine,
                Encoding = _encoding ?? Encoding.UTF8,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
            };

            _out = System.Xml.XmlWriter.Create(_writer, writerSettings);
            _outputHeader = !_config.SuppressHeader;
        }

        /// <summary>
        /// Writes a record object to this output stream.
        /// </summary>
        /// <param name="record">Record the record object to write</param>
        public void Write(object record)
        {
            // write the XMl header if needed
            if (_outputHeader)
            {
                var pi = new StringBuilder();
                if (_config.Version != null)
                    pi.AppendFormat(" version=\"{0}\"", _config.Version);
                if (_encoding != null)
                    pi.AppendFormat(" encoding=\"{0}\"", _config.Encoding);
                _out.WriteProcessingInstruction("xml", pi.ToString().TrimStart());
                _outputHeader = false;
            }

            // a null record indicates we need to close an element
            if (record == null)
            {
                if (_elementStack != null)
                {
                    EndElement();
                }
            }
            else
            {
                // otherwise we write the record (i.e. DOM tree) to the stream
                Write(((XDocument)record).Root);
            }
        }

        /// <summary>
        /// Flushes the output stream.
        /// </summary>
        public void Flush()
        {
            _out.Flush();
        }

        /// <summary>
        /// Closes the output stream.
        /// </summary>
        public void Close()
        {
            while (_elementStack != null)
            {
                EndElement();
            }

            _out.WriteEndDocument();
            _out.Flush();
            _out.Dispose();

            _writer.Flush();
            _writer.Dispose();
        }

        /// <summary>
        /// Updates a <see cref="IDictionary{TKey,TValue}"/> with the current state of the Writer to allow for
        /// restoration at a later time
        /// </summary>
        /// <param name="ns">a string to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> to update with the latest state</param>
        public void UpdateState(string ns, IDictionary<string, object> state)
        {
            state[GetKey(ns, OUTPUT_HEADER_KEY)] = _outputHeader;
            state[GetKey(ns, NAMESPACE_MAP_KEY)] = ToToken(_namespaceMap);

            object n;

            if (!state.TryGetValue(GetKey(ns, LEVEL_KEY), out n))
                n = 0;
            var lastLevel = Convert.ToInt32(n);

            // remove previous stack items beyond the current level
            for (var i = lastLevel; i > _level; i--)
            {
                var stackPrefix = $"{ns}.s{i}";
                state.Remove(GetKey(stackPrefix, STACK_ELEMENT_KEY));
                state.Remove(GetKey(stackPrefix, STACK_NS_MAP_KEY));
            }

            int to = DELTA_ENABLED ? _dirtyLevel : 0;

            // update dirtied stack items up to the current level
            var e = _elementStack;
            for (int i = _level; i > to; i--)
            {
                var stackPrefix = $"{ns}.s{i}";
                state[GetKey(stackPrefix, STACK_ELEMENT_KEY)] = e.ToToken();

                var nsMapKey = GetKey(stackPrefix, STACK_NS_MAP_KEY);
                var token = ToToken(e.Namespaces);
                if (token == null)
                {
                    state.Remove(nsMapKey);
                }
                else
                {
                    state[nsMapKey] = token;
                }

                e = _elementStack.Parent;
            }

            _dirtyLevel = _level;

            state[GetKey(ns, LEVEL_KEY)] = _level;
        }

        /// <summary>
        /// Restores a <see cref="IDictionary{TKey,TValue}"/> of previously stored state information
        /// </summary>
        /// <param name="ns">a string to prefix all state keys with</param>
        /// <param name="state">the <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing the state to restore</param>
        public void RestoreState(string ns, IReadOnlyDictionary<string, object> state)
        {
            object o;
            _outputHeader = (bool)GetRequired(ns, OUTPUT_HEADER_KEY, state);
            var key = GetKey(ns, NAMESPACE_MAP_KEY);
            if (!state.TryGetValue(key, out o))
                o = null;
            var token = (string)o;
            if (token != null)
            {
                _namespaceMap = ToMap(token, key);
                _namespaceIndex = _namespaceMap.Count;
                _usedPrefixes = new HashSet<string>(_namespaceMap.Values);
            }
            else
            {
                _namespaceMap.Clear();
                _namespaceIndex = 0;
                _usedPrefixes.Clear();
            }

            _level = 0;
            _elementStack = null;

            try
            {
                _out.Flush();
                _writer.SuppressOutput = true;

                var level = (int)GetRequired(ns, LEVEL_KEY, state);
                for (int i = 0; i != level; ++i)
                {
                    var stackPrefix = $"{ns}.s{i + 1}";

                    var e = ElementStack.FromToken(_elementStack, (string)GetRequired(stackPrefix, STACK_ELEMENT_KEY, state));
                    if (e.IsDefaultNamespace())
                    {
                        _out.WriteStartElement(e.Name);
                    }
                    else if (string.IsNullOrEmpty(e.Prefix))
                    {
                        _out.WriteStartElement(e.Name, e.Namespace);
                    }
                    else
                    {
                        _out.WriteStartElement(e.Prefix, e.Name, e.Namespace);
                    }

                    // create a stack item
                    Push(e);

                    // add namespaces
                    var nsMap = (string)state[GetKey(stackPrefix, STACK_NS_MAP_KEY)];
                    if (nsMap != null)
                    {
                        var s = nsMap.Trim().Split(' ');
                        if ((s.Length & 1) != 0)
                        {
                            throw new InvalidOperationException($"Invalid state information for key '{GetKey(stackPrefix, STACK_NS_MAP_KEY)}'");
                        }

                        Debug.Assert(_elementStack != null, "_elementStack != null");
                        for (var n = 0; n < s.Length; n += 2)
                        {
                            _elementStack.AddNamespace(s[n + 1], s[n]);
                            _out.WriteAttributeString("xmlns", s[n + 1], XNamespace.Xmlns.NamespaceName, s[n]);
                        }
                    }
                }
            }
            finally
            {
                _writer.SuppressOutput = false;
            }

            _dirtyLevel = _level;
        }

        private static string GetKey(string ns, string name)
        {
            return $"{ns}.{name}";
        }

        /// <summary>
        /// Retrieves a value from a Map for a given key prepended with the namespace
        /// </summary>
        /// <param name="ns">a string to prefix all state keys with</param>
        /// <param name="key">the key to build the namespace prefixed key for</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> containing the state to restore</param>
        /// <returns>the value for the given <paramref name="ns"/> and <paramref name="key"/></returns>
        private static object GetRequired(string ns, string key, IReadOnlyDictionary<string, object> state)
        {
            key = GetKey(ns, key);
            object value;
            if (!state.TryGetValue(key, out value))
                throw new InvalidOperationException($"Missing state information for key '{key}'");
            return value;
        }

        /// <summary>
        /// Constructs a Map from a String of space delimited key-values pairings
        /// </summary>
        /// <param name="token">the token to parse</param>
        /// <param name="key">the key (for informational purposes only)</param>
        /// <returns>the new dictionary containing the data from the token</returns>
        private static Dictionary<string, string> ToMap([CanBeNull] string token, string key)
        {
            if (token == null)
                return null;

            var s = token.Trim().Split(' ');
            if ((s.Length & 1) == 1)
                throw new InvalidOperationException($"Invalid state information for key '{key}'");

            var map = new Dictionary<string, string>();
            for (var n = 0; n != s.Length; n += 2)
                map[s[n]] = s[n + 1];

            return map;
        }

        /// <summary>
        /// Converts a Map to a String of space delimited key-value pairings
        /// </summary>
        /// <param name="map">the dictionary to serialize</param>
        /// <returns>the serialized dictionary</returns>
        private static string ToToken(IReadOnlyDictionary<string, string> map)
        {
            if (map == null || map.Count == 0)
                return null;

            var first = true;
            var token = new StringBuilder();
            foreach (var entry in map)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    token.Append(' ');
                }

                token.AppendFormat("{0} {1}", entry.Key, entry.Value);
            }

            return token.ToString();
        }

        /// <summary>
        /// Recursively writes an element to the XML stream writer
        /// </summary>
        /// <param name="element">the DOM element to write</param>
        [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP2101:MethodMustNotContainMoreLinesThan", Justification = "Reviewed. Suppression is OK here.")]
        private void Write(XElement element)
        {
            var name = element.Name.LocalName;
            string prefix;
            var ns = element.Name.NamespaceName;

            var nsHandling = NamespaceAwareElementComparer.GetHandlingModeFor(element);

            if (nsHandling == NamespaceHandlingMode.NoPrefix)
            {
                prefix = null;
            }
            else
            {
                prefix = element.GetPrefixOfNamespace(element.Name.Namespace);
            }

            var ignoreNamespace = false;
            if (string.IsNullOrEmpty(ns))
            {
                if (nsHandling == NamespaceHandlingMode.IgnoreNamespace)
                {
                    prefix = null;
                    ignoreNamespace = true;
                }

                ns = string.Empty;
            }

            var setDefaultNamespace = false;
            if (string.IsNullOrEmpty(prefix) && !ignoreNamespace)
            {
                if (nsHandling == NamespaceHandlingMode.DefaultNamespace)
                {
                    setDefaultNamespace = true;
                }
            }

            // flag for lazily appending to stack
            var pendingStackUpdate = true;

            var namespacesToAdd = new List<Tuple<string, string>>();

            // start the element
            if (_elementStack == null)
            {
                if (ignoreNamespace)
                {
                    _out.WriteStartElement(name);
                }
                else if (!string.IsNullOrEmpty(prefix))
                {
                    _out.WriteStartElement(prefix, name, ns);
                }
                else
                {
                    _out.WriteStartElement(string.Empty, name, ns ?? string.Empty);
                }

                Push(ns, prefix, name);
                if (_config.NamespaceMap != null)
                {
                    Debug.Assert(_elementStack != null, "_elementStack != null");
                    foreach (var entry in _config.NamespaceMap)
                    {
                        namespacesToAdd.Add(Tuple.Create(entry.Key, entry.Value));
                        _elementStack.AddNamespace(entry.Key, entry.Value);
                    }
                }

                pendingStackUpdate = false;
            }
            else
            {
                if (ignoreNamespace || (_elementStack.IsDefaultNamespace(ns) && string.IsNullOrEmpty(prefix)))
                {
                    _out.WriteStartElement(name);
                    ns = _elementStack.DefaultNamespace;
                    prefix = null;
                }
                else
                {
                    var p = _elementStack.FindPrefix(ns);

                    if (!string.IsNullOrEmpty(p) && string.IsNullOrEmpty(prefix) && !setDefaultNamespace && nsHandling != NamespaceHandlingMode.NoPrefix)
                    {
                        prefix = p;
                    }

                    if (string.IsNullOrEmpty(prefix))
                    {
                        _out.WriteStartElement(string.Empty, name, ns);
                    }
                    else
                    {
                        _out.WriteStartElement(prefix, name, ns);
                    }
                }
            }

            var namespaceInsertIndex = 0;
            var attributesToAdd = new List<Tuple<string, XName, string>>();

            // write attributes
            ISet<string> attPrefixSet = null;

            var map = element.Attributes().ToList();
            if (map.Count > 0)
            {
                if (pendingStackUpdate)
                {
                    Push(ns, prefix, name);
                    pendingStackUpdate = false;
                }
            }

            for (int i = 0, j = map.Count; i < j; i++)
            {
                var att = map[i];
                var attName = att.Name.LocalName;
                if (attName == "xmlns")
                    continue;
                var attNamespace = att.Name.NamespaceName;
                var attPrefix = element.GetPrefixOfNamespace(attNamespace);

                if (string.IsNullOrEmpty(attNamespace))
                {
                    attributesToAdd.Add(new Tuple<string, XName, string>(null, attName, att.Value));
                }
                else if (nsHandling != NamespaceHandlingMode.NoPrefix || !string.Equals(attPrefix, "xmlns") || !string.Equals(attNamespace, ns, StringComparison.Ordinal))
                {
                    var p = _elementStack.FindPrefix(attNamespace);

                    var declareNamespace = false;
                    if (p == null)
                    {
                        if (attPrefix == null)
                        {
                            _namespaceMap.TryGetValue(attNamespace, out attPrefix);
                            if (attPrefix == null)
                            {
                                attPrefix = CreateNamespace(attNamespace);
                            }
                        }

                        if (attPrefix != "xmlns" && (attPrefixSet == null || !attPrefixSet.Contains(attPrefix)))
                        {
                            declareNamespace = true;
                        }
                    }
                    else if (attPrefix == null)
                    {
                        attPrefix = p;
                    }

                    if (declareNamespace)
                    {
                        if (attPrefixSet == null)
                        {
                            attPrefixSet = new HashSet<string>();
                        }

                        attPrefixSet.Add(attPrefix);
                        namespacesToAdd.Insert(namespaceInsertIndex++, Tuple.Create(attPrefix, attNamespace));
                    }

                    if (attPrefix == "xmlns")
                    {
                        namespacesToAdd.Insert(namespaceInsertIndex++, Tuple.Create(attName, att.Value));
                    }
                    else
                    {
                        attributesToAdd.Add(new Tuple<string, XName, string>(attPrefix, XName.Get(attName, attNamespace), att.Value));
                    }
                }
            }

            var addedNamespacePrefixes = new HashSet<string>();
            foreach (var namespaceToAdd in namespacesToAdd)
            {
                if (addedNamespacePrefixes.Add(namespaceToAdd.Item1))
                    _out.WriteAttributeString("xmlns", namespaceToAdd.Item1, XNamespace.Xmlns.NamespaceName, namespaceToAdd.Item2);
            }

            foreach (var attributeToAdd in attributesToAdd)
            {
                var attPrefix = attributeToAdd.Item1;
                var attName = attributeToAdd.Item2;
                var attValue = attributeToAdd.Item3;
                if (attPrefix != null)
                {
                    _out.WriteAttributeString(attPrefix, attName.LocalName, attName.NamespaceName, attValue);
                }
                else
                {
                    _out.WriteAttributeString(attName.LocalName, attName.NamespaceName, attValue);
                }
            }

            // write children
            var child = element.FirstNode;
            while (child != null)
            {
                switch (child.NodeType)
                {
                    case XmlNodeType.Element:
                        if (pendingStackUpdate)
                        {
                            Push(ns, prefix, name);
                            pendingStackUpdate = false;
                        }

                        Write((XElement)child);
                        break;

                    case XmlNodeType.Text:
                        _out.WriteString(((XText)child).Value);
                        break;
                    case XmlNodeType.CDATA:
                        _out.WriteCData(((XCData)child).Value);
                        break;
                }

                child = child.NextNode;
            }

            // end the element if it is not a group
            var isGroupElementAnnotation = element.Annotation<IsGroupElementAnnotation>();
            var isGroupElement = isGroupElementAnnotation != null && isGroupElementAnnotation.IsGroupElement;
            if (!isGroupElement)
            {
                if (!pendingStackUpdate)
                {
                    Pop();
                }

                _out.WriteEndElement();
            }
        }

        private void EndElement()
        {
            Pop();
            _out.WriteEndElement();
        }

        private void Push(string ns, string prefix, string name)
        {
            Push(new ElementStack(_elementStack, ns, prefix, name));
        }

        private void Push(ElementStack e)
        {
            _elementStack = e;
            ++_level;
        }

        private void Pop()
        {
            _elementStack = _elementStack.Parent;
            --_level;
            _dirtyLevel = Math.Min(_dirtyLevel, _level);
        }

        private string CreateNamespace(string uri)
        {
            string prefix;
            if (uri == XmlNodeUtil.Xsi.NamespaceName)
            {
                prefix = Settings.Instance.GetProperty(Settings.DEFAULT_XSI_NAMESPACE_PREFIX) ?? "xsi";
            }
            else
            {
                prefix = $"ns{++_namespaceIndex}";
            }

            while (!_usedPrefixes.Add(prefix))
            {
                prefix = $"ns{++_namespaceIndex}";
            }

            _namespaceMap.Add(uri, prefix);

            return prefix;
        }

        private class FilterWriter : TextWriter
        {
            private readonly TextWriter _baseWriter;

            public FilterWriter(TextWriter baseWriter)
            {
                _baseWriter = baseWriter;
            }

            public bool SuppressOutput { private get; set; }

            public override Encoding Encoding => _baseWriter.Encoding;

            public override void Write(char value)
            {
                if (!SuppressOutput)
                    _baseWriter.Write(value);
            }

            public override void WriteLine(string value)
            {
                if (!SuppressOutput)
                    base.WriteLine(value);
            }

            public override void WriteLine(char[] buffer, int index, int count)
            {
                if (!SuppressOutput)
                    base.WriteLine(buffer, index, count);
            }
        }
    }
}
