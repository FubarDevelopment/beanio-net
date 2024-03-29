// <copyright file="XmlMappingParser.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using BeanIO.Builder;
using BeanIO.Config;
using BeanIO.Internal.Config.Annotation;
using BeanIO.Internal.Util;
using BeanIO.Stream;

namespace BeanIO.Internal.Config.Xml
{
    /// <summary>
    /// Parses a mapping file into <see cref="BeanIOConfig"/> objects.
    /// </summary>
    /// <remarks>
    /// <para>A <see cref="BeanIOConfig"/> is produced for each mapping file imported by the
    /// mapping file being parsed, and the entire collection is returned from <see cref="LoadConfiguration"/>.</para>
    /// <para>This class is not thread safe and a new instance should be created for parsing
    /// each input stream.</para>
    /// </remarks>
    internal class XmlMappingParser : IPropertySource
    {
        private static readonly bool _propertySubstitutionEnabled = Settings.Instance.GetBoolean(Settings.PROPERTY_SUBSTITUTION_ENABLED);

        /// <summary>
        /// used to read XML into a DOM object.
        /// </summary>
        private readonly XmlMappingReader _reader;

        private readonly Stack<Include> _includes = new Stack<Include>();

        /// <summary>
        /// custom Properties provided by the client for property expansion.
        /// </summary>
        private Properties? _properties;

        /// <summary>
        /// the mapping currently being parsed.
        /// </summary>
        private XmlMapping _mapping;

        /// <summary>
        /// a Dictionary of all loaded mappings (including the root).
        /// </summary>
        private Dictionary<Uri, XmlMapping> _mappings;

        /// <summary>
        /// whether the current record class was annotated.
        /// </summary>
        private bool _annotatedRecord = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlMappingParser"/> class.
        /// </summary>
        /// <param name="reader">the XML mapping reader for reading XML mapping files into a DOM object.</param>
        public XmlMappingParser(XmlMappingReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            (_mappings, _mapping) = CreateDefaultMappings();
        }

        /// <summary>
        /// Gets the mapping file information actively being parsed, which may change
        /// when one mapping file imports another.
        /// </summary>
        protected XmlMapping Mapping => _mapping;

        /// <summary>
        /// Gets the amount to offset a field position, which is calculated
        /// according to included template offset configurations.
        /// </summary>
        protected int PositionOffset
        {
            get
            {
                if (_includes.Count == 0)
                    return 0;
                return _includes.Peek().Offset;
            }
        }

        /// <summary>
        /// Reads a mapping file input stream and returns a collection of BeanIO
        /// configurations, one for the input stream and one for each imported
        /// mapping file (if specified).
        /// </summary>
        /// <param name="input">the input stream to read from.</param>
        /// <param name="properties">the <see cref="Properties"/> to use for property substitution.</param>
        /// <returns>the collection of parsed BeanIO configuration objects.</returns>
        public ICollection<BeanIOConfig> LoadConfiguration(System.IO.Stream input, Properties? properties)
        {
            _properties = properties;
            (_mappings, _mapping) = CreateDefaultMappings();

            try
            {
                LoadMapping(input);
            }
            catch (BeanIOConfigurationException ex) when (_mapping.Location != null)
            {
                throw new BeanIOConfigurationException($"Invalid mapping file '{_mapping.Name}': {ex.Message}", ex);
            }

            var configList = new List<BeanIOConfig>(_mappings.Count);
            foreach (var mapping in _mappings.Values)
            {
                var config = mapping.Configuration.Clone();

                // global type handlers are the only elements that need to be imported
                // from other mapping files
                var handlers = new List<TypeHandlerConfig>();
                mapping.AddTypeHandlersTo(handlers);

                config.TypeHandlerConfigurations = handlers;
                configList.Add(config);
            }

            return configList;
        }

        /// <summary>
        /// Returns the property value for a given key.
        /// </summary>
        /// <param name="key">the property key.</param>
        /// <returns>the property value.</returns>
        public string? GetProperty(string key)
        {
            string? value = null;
            if (_properties != null)
            {
                value = _properties[key];
            }

            if (value == null)
            {
                var mappingProperties = _mapping.Properties;
                if (mappingProperties != null)
                {
                    value = mappingProperties[key];
                }
            }

            return value;
        }

        /// <summary>
        /// Includes a template.
        /// </summary>
        /// <param name="config">the parent bean configuration.</param>
        /// <param name="element">the <c>include</c> DOM element to parse.</param>
        public void IncludeTemplate(ComponentConfig config, XElement element)
        {
            var template =
                GetAttribute(element, "template")
                ?? throw new InvalidOperationException($"Template attribute not specified for element '{element}'");
            var offset = GetIntAttribute(element, "offset") ?? 0;
            IncludeTemplate(config, template, offset);
        }

        /// <summary>
        /// Includes a template.
        /// </summary>
        /// <param name="config">the parent bean configuration.</param>
        /// <param name="template">the name of the template to include.</param>
        /// <param name="offset">the value to offset configured positions by.</param>
        public void IncludeTemplate(ComponentConfig config, string template, int offset)
        {
            var element = _mapping.FindTemplate(template);

            // validate the template was declared
            if (element == null)
                throw new BeanIOConfigurationException($"Template '{template}' not found");

            // validate there is no circular reference
            foreach (var include in _includes)
            {
                if (string.Equals(template, include.Template, StringComparison.Ordinal))
                    throw new BeanIOConfigurationException($"Circular reference detected in template '{template}'");
            }

            // adjust the configured offset by any previous offset
            offset += PositionOffset;

            Include inc = new Include(template, offset);
            _includes.Push(inc);
            AddProperties(config, element);
            _includes.Pop();
        }

        /// <summary>
        /// Initiates the parsing of an imported mapping file.
        /// </summary>
        /// <remarks>
        /// After parsing completes, <see cref="Pop"/> must be invoked before continuing.
        /// </remarks>
        /// <param name="name">the name of the imported mapping file.</param>
        /// <param name="location">the location of the imported mapping file
        /// (this should be an absolute URL so that importing the
        /// same mapping more than once can be detected).</param>
        /// <returns>the new Mapping object pushed onto the stack
        /// (this can also be accessed by calling <see cref="Mapping"/>.</returns>
        protected XmlMapping Push(string name, Uri location)
        {
            var m = new XmlMapping(name, location, _mapping);
            _mappings[location] = m;
            _mapping.AddImport(m);
            _mapping = m;
            return _mapping;
        }

        /// <summary>
        /// Completes the parsing of an imported mapping file.
        /// </summary>
        /// <seealso cref="Push"/>
        protected void Pop()
        {
            _mapping = _mapping.Parent ?? throw new InvalidOperationException("XML mapping parser stack is empty");
        }

        /// <summary>
        /// Loads a mapping file from an input stream.
        /// </summary>
        /// <param name="input">the input stream to read from.</param>
        protected virtual void LoadMapping(System.IO.Stream input)
        {
            var doc = _reader.LoadDocument(input);
            LoadMapping(doc.Root ?? throw new InvalidOperationException("XML document contains no root element"));
        }

        /// <summary>
        /// Parses a BeanIO configuration from a DOM element.
        /// </summary>
        /// <param name="element">the root <c>beanio</c> DOM element to parse.</param>
        protected virtual void LoadMapping(XElement element)
        {
            var config = _mapping.Configuration;
            foreach (var child in element.Elements())
            {
                var name = child.Name.LocalName;
                switch (name)
                {
                    case "import":
                        ImportConfiguration(child);
                        break;
                    case "property":
                        {
                            var key = GetAttribute(child, "name") ?? throw new InvalidOperationException("\"name\" attribute missing or empty");
                            var value = GetAttribute(child, "value");
                            _mapping.SetProperty(key, value);
                        }

                        break;
                    case "typeHandler":
                        {
                            TypeHandlerConfig handler = CreateHandlerConfig(child);
                            if (handler.Name != null && _mapping.IsDeclaredGlobalTypeHandler(handler.Name))
                                throw new BeanIOConfigurationException($"Duplicate global type handler named '{handler.Name}'");
                            config.Add(handler);
                        }

                        break;
                    case "template":
                        CreateTemplate(child);
                        break;
                    case "stream":
                        config.Add(CreateStreamConfig(child));
                        break;
                }
            }
        }

        /// <summary>
        /// Parses an <c>import</c> DOM element and loads its mapping file.
        /// </summary>
        /// <param name="element">the <c>import</c> DOM element.</param>
        /// <returns>a new <see cref="XmlMapping"/> for the imported resource or file.</returns>
        protected XmlMapping ImportConfiguration(XElement element)
        {
            var resource = GetAttribute(element, "resource")
                           ?? throw new BeanIOConfigurationException($"Element {element} contains no resource attribute");
            var name = resource;

            var colonIndex = resource.IndexOf(':');
            if (colonIndex == -1)
                throw new BeanIOConfigurationException($"No scheme specified for resource '{resource}'");

            var url = new Uri(resource);
            var handler = Settings.Instance.GetSchemeHandler(url, false);
            if (handler == null)
            {
                throw new BeanIOConfigurationException(
                    $"Scheme of import resource name {resource} must one of: {string.Join(", ", Settings.Instance.SchemeHandlers.Keys.Select(x => $"'{x}'"))}");
            }

            if (_mapping.IsLoading(url))
                throw new BeanIOConfigurationException($"Failed to import resource '{name}': Circular reference(s) detected");

            // check to see if the mapping file has already been loaded
            if (_mappings.TryGetValue(url, out var m))
            {
                _mapping.AddImport(m);
                return m;
            }

            try
            {
                using (var input = handler.Open(url))
                {
                    if (input == null)
                        throw new BeanIOConfigurationException($"Resource '{name}' not found in classpath for import");

                    // push a new Mapping instance onto the stack for this url
                    Push(name, url).Configuration.Source = name;

                    LoadMapping(input);

                    // this is purposely not put in a finally block so that
                    // calling methods can know the mapping file that errored
                    // if a BeanIOConfigurationException is thrown
                    Pop();
                    return _mapping;
                }
            }
            catch (IOException ex)
            {
                throw new BeanIOConfigurationException($"Failed to import mapping file '{name}'", ex);
            }
        }

        /// <summary>
        /// Parses a <see cref="TypeHandlerConfig"/> from a DOM element.
        /// </summary>
        /// <param name="element">the DOM element to parse.</param>
        /// <returns>the new <see cref="TypeHandlerConfig"/>.</returns>
        protected virtual TypeHandlerConfig CreateHandlerConfig(XElement element)
        {
            var config = new TypeHandlerConfig
                {
                    Name = GetAttribute(element, "name"),
                    Type = GetAttribute(element, "type"),
                    ClassName = GetAttribute(element, "class"),
                    Format = GetAttribute(element, "format"),
                    Properties = CreateProperties(element),
                };
            return config;
        }

        /// <summary>
        /// Adds a template to the active mapping.
        /// </summary>
        /// <param name="element">the DOM element that defines the template.</param>
        protected virtual void CreateTemplate(XElement element)
        {
            var templateName =
                GetAttribute(element, "name")
                ?? throw new BeanIOConfigurationException($"Element {element} contains nor template name attribute");
            if (!_mapping.AddTemplate(templateName, element))
                throw new BeanIOConfigurationException($"Duplicate template named '{templateName}'");
        }

        /// <summary>
        /// Parses a <c>Bean</c> from a DOM element.
        /// </summary>
        /// <typeparam name="T">the bean type.</typeparam>
        /// <param name="element">the DOM element to parse.</param>
        /// <returns>the new <c>Bean</c>.</returns>
        protected BeanConfig<T> CreateBeanFactory<T>(XElement element)
        {
            return new BeanConfig<T>()
                {
                    ClassName = GetAttribute(element, "class"),
                    Properties = CreateProperties(element),
                };
        }

        /// <summary>
        /// Parses <see cref="Properties"/> from a DOM element.
        /// </summary>
        /// <param name="element">the DOM element to parse.</param>
        /// <returns>the new <see cref="Properties"/>.</returns>
        protected Properties CreateProperties(XElement element)
        {
            var props = new Dictionary<string, string?>();
            foreach (var child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "property":
                        var name = GetAttribute(child, "name")
                                   ?? throw new BeanIOConfigurationException($"Element contains nor name attribute");
                        var value = GetAttribute(child, "value");
                        props[name] = value ?? string.Empty;
                        break;
                }
            }

            return new Properties(props);
        }

        /// <summary>
        /// Parses a field configuration from a DOM element.
        /// </summary>
        /// <param name="element">the <c>field</c> DOM element to parse.</param>
        /// <returns>the parsed field configuration.</returns>
        protected FieldConfig CreateFieldConfig(XElement element)
        {
            var config = new FieldConfig()
            {
                Name = GetAttribute(element, "name"),
                MinLength = GetIntAttribute(element, "minLength"),
                MaxLength = GetUnboundedIntAttribute(element, "maxLength"),
                RegEx = GetAttribute(element, "regex"),
                Literal = GetAttribute(element, "literal"),
                TypeHandler = GetTypeHandler(element, "typeHandler"),
                Type = GetAttribute(element, "type"),
                Format = GetAttribute(element, "format"),
                DefaultValue = GetOptionalAttribute(element, "default"),
                Length = GetUnboundedIntAttribute(element, "length", -1),
                Padding = GetCharacterAttribute(element, "padding"),
                XmlType = GetEnumAttribute<XmlNodeType>(element, "xmlType"),
                XmlName = GetAttribute(element, "xmlName"),
                XmlNamespace = GetOptionalAttribute(element, "xmlNamespace"),
                XmlPrefix = GetOptionalAttribute(element, "xmlPrefix")
            };

            PopulatePropertyConfig(config, element);

            var position = GetIntAttribute(element, "at");
            if (position == null)
            {
                position = GetIntAttribute(element, "position");
            }
            else if (HasAttribute(element, "position"))
            {
                throw new BeanIOConfigurationException("Only one of 'position' or 'at' can be configured");
            }

            if (position != null)
            {
                if (position >= 0)
                    position += PositionOffset;
                config.Position = position;
            }

            config.Until = GetIntAttribute(element, "until");
            config.IsRequired = GetBoolAttribute(element, "required") ?? config.IsRequired;
            config.IsTrim = GetBoolAttribute(element, "trim") ?? config.IsTrim;
            config.IsLazy = GetBoolAttribute(element, "lazy") ?? config.IsLazy;
            config.IsIdentifier = GetBoolAttribute(element, "rid") ?? config.IsIdentifier;
            config.IsBound = !(GetBoolAttribute(element, "ignore") ?? false);
            config.KeepPadding = GetBoolAttribute(element, "keepPadding") ?? config.KeepPadding;
            config.IsLenientPadding = GetBoolAttribute(element, "lenientPadding") ?? config.IsLenientPadding;
            config.IsNillable = GetBoolAttribute(element, "nillable") ?? config.IsNillable;
            config.ParseDefault = GetBoolAttribute(element, "parseDefault") ?? config.ParseDefault;

            if (HasAttribute(element, "justify"))
            {
                if (HasAttribute(element, "align"))
                    throw new BeanIOConfigurationException("Only one of 'align' or 'justify' can be configured");
                config.Justify = GetEnumAttribute<Align>(element, "justify") ?? Align.Left;
            }
            else
            {
                config.Justify = GetEnumAttribute<Align>(element, "align") ?? Align.Left;
            }

            return config;
        }

        /// <summary>
        /// Parses a constant component configuration from a DOM element.
        /// </summary>
        /// <param name="element">the <c>property</c> DOM element to parse.</param>
        /// <returns>the parsed constant configuration.</returns>
        protected ConstantConfig CreateConstantConfig(XElement element)
        {
            var name = GetAttribute(element, "name");
            try
            {
                var config = new ConstantConfig()
                    {
                        Name = GetAttribute(element, "name"),
                        Getter = GetAttribute(element, "getter"),
                        Setter = GetAttribute(element, "setter"),
                        Type = GetAttribute(element, "type"),
                        TypeHandler = GetTypeHandler(element, "typeHandler"),
                        Format = GetAttribute(element, "format"),
                        Value = GetAttribute(element, "value"),
                    };

                config.IsIdentifier = GetBoolAttribute(element, "rid") ?? config.IsIdentifier;

                return config;
            }
            catch (Exception ex)
            {
                throw new BeanIOConfigurationException($"Invalid '{name}' property definition: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parses a <see cref="StreamConfig"/> from a DOM element.
        /// </summary>
        /// <param name="element">the <c>stream</c> DOM element to parse.</param>
        /// <returns>the new <see cref="StreamConfig"/>.</returns>
        protected StreamConfig CreateStreamConfig(XElement element)
        {
            var config = new StreamConfig()
                {
                    Name = GetAttribute(element, "name"),
                    Format = GetAttribute(element, "format"),
                    Mode = GetEnumAttribute<AccessMode>(element, "mode"),
                    ValidateOnMarshal = GetBoolAttribute(element, "validateOnMarshal"),
                    XmlName = GetAttribute(element, "xmlName"),
                    XmlNamespace = GetOptionalAttribute(element, "xmlNamespace"),
                    XmlPrefix = GetOptionalAttribute(element, "xmlPrefix"),
                    XmlType = GetOptionalEnumAttribute<XmlNodeType>(element, "xmlType"),
                    ResourceBundle = GetAttribute(element, "resourceBundle"),
                };
            config.IsStrict = GetBoolAttribute(element, "strict") ?? config.IsStrict;
            config.IgnoreUnidentifiedRecords = GetBoolAttribute(element, "ignoreUnidentifiedRecords") ?? config.IgnoreUnidentifiedRecords;
            PopulatePropertyConfigOccurs(config, element);

            foreach (var child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "typeHandler":
                        config.AddHandler(CreateHandlerConfig(child));
                        break;
                    case "parser":
                        config.ParserFactory = CreateBeanFactory<IRecordParserFactory>(child);
                        break;
                    case "record":
                        config.Add(CreateRecordConfig(child));
                        break;
                    case "group":
                        config.Add(CreateGroupConfig(child));
                        break;
                }
            }

            return config;
        }

        /// <summary>
        /// Parses a group configuration from a DOM element.
        /// </summary>
        /// <param name="element">the <c>group</c> DOM element to parse.</param>
        /// <returns>the parsed group configuration.</returns>
        protected GroupConfig CreateGroupConfig(XElement element)
        {
            var typeName = GetAttribute(element, "class");
            var config = AnnotationParser.CreateGroupConfig(typeName);
            if (config != null)
                return config;

            config = new GroupConfig()
            {
                Name = GetAttribute(element, "name"),
                Type = typeName,
                Order = GetIntAttribute(element, "order"),
                Target = GetAttribute(element, "value"),
                XmlName = GetAttribute(element, "xmlName"),
                XmlNamespace = GetOptionalAttribute(element, "xmlNamespace"),
                XmlPrefix = GetOptionalAttribute(element, "xmlPrefix"),
                XmlType = GetOptionalEnumAttribute<XmlNodeType>(element, "xmlType"),
            };
            PopulatePropertyConfig(config, element);
            config.SetKey(GetAttribute(element, "key"));

            foreach (var child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "record":
                        config.Add(CreateRecordConfig(child));
                        break;
                    case "group":
                        config.Add(CreateGroupConfig(child));
                        break;
                }
            }

            return config;
        }

        /// <summary>
        /// Parses a record configuration from the given DOM element.
        /// </summary>
        /// <param name="element">the <c>record</c> DOM element to parse.</param>
        /// <returns>the parsed record configuration.</returns>
        protected RecordConfig CreateRecordConfig(XElement element)
        {
            var typeName = GetAttribute(element, "class");
            var config = AnnotationParser.CreateRecordConfig(typeName);
            if (config != null)
                return config;

            config = new RecordConfig()
                {
                    Name = GetAttribute(element, "name"),
                    Type = typeName,
                    Order = GetIntAttribute(element, "order"),
                    MinLength = GetIntAttribute(element, "minLength"),
                    MaxLength = GetUnboundedIntAttribute(element, "maxLength", int.MaxValue),
                    XmlName = GetAttribute(element, "xmlName"),
                    XmlNamespace = GetOptionalAttribute(element, "xmlNamespace"),
                    XmlPrefix = GetOptionalAttribute(element, "xmlPrefix"),
                    XmlType = GetOptionalEnumAttribute<XmlNodeType>(element, "xmlType"),
                };
            PopulatePropertyConfig(config, element);
            config.SetKey(GetAttribute(element, "key"));
            config.IsLazy = GetBoolAttribute(element, "lazy") ?? config.IsLazy;
            if (HasAttribute(element, "value"))
            {
                config.Target = GetAttribute(element, "value");
                if (HasAttribute(element, "target"))
                    throw new BeanIOConfigurationException("Only one 'value' or 'target' can be configured");
            }
            else
            {
                config.Target = GetAttribute(element, "target");
            }

            var range = GetRangeAttribute(element, "ridLength");
            if (range != null)
            {
                config.MinMatchLength = range.Min;
                config.MaxMatchLength = range.Max;
            }

            var template = GetOptionalAttribute(element, "template");
            if (template != null)
                IncludeTemplate(config, template, 0);

            AddProperties(config, element);

            return config;
        }

        /// <summary>
        /// Parses a segment component configuration from a DOM element.
        /// </summary>
        /// <param name="element">the <c>segment</c> DOM element to parse.</param>
        /// <returns>the parsed segment configuration.</returns>
        protected SegmentConfig CreateSegmentConfig(XElement element)
        {
            var config = new SegmentConfig()
                {
                    Name = GetAttribute(element, "name"),
                    Type = GetAttribute(element, "class"),
                    XmlName = GetAttribute(element, "xmlName"),
                    XmlNamespace = GetOptionalAttribute(element, "xmlNamespace"),
                    XmlPrefix = GetOptionalAttribute(element, "xmlPrefix"),
                    XmlType = GetOptionalEnumAttribute<XmlNodeType>(element, "xmlType"),
                };

            PopulatePropertyConfig(config, element);
            config.SetKey(GetAttribute(element, "key"));

            config.IsLazy = GetBoolAttribute(element, "lazy") ?? config.IsLazy;
            config.IsNillable = GetBoolAttribute(element, "nillable") ?? config.IsLazy;

            var template = GetOptionalAttribute(element, "template");
            if (template != null)
                IncludeTemplate(config, template, 0);
            AddProperties(config, element);

            if (HasAttribute(element, "value"))
            {
                config.Target = GetAttribute(element, "value");
                if (HasAttribute(element, "target"))
                    throw new BeanIOConfigurationException("Only one of 'value' or 'target' can be configured");
            }
            else
            {
                config.Target = GetAttribute(element, "target");
            }

            return config;
        }

        /// <summary>
        /// Parses bean properties from the given DOM element.
        /// </summary>
        /// <param name="config">the enclosing bean configuration to add the properties to.</param>
        /// <param name="element">the <c>bean</c> or <c>record</c> DOM element to parse.</param>
        protected void AddProperties(ComponentConfig config, XElement element)
        {
            foreach (var child in element.Elements())
            {
                if (_annotatedRecord && config.ComponentType == ComponentType.Record)
                    throw new BeanIOConfigurationException("Annotated classes may not contain child componennts in a mapping file");

                switch (child.Name.LocalName)
                {
                    case "field":
                        config.Add(CreateFieldConfig(child));
                        break;
                    case "segment":
                        config.Add(CreateSegmentConfig(child));
                        break;
                    case "property":
                        config.Add(CreateConstantConfig(child));
                        break;
                    case "include":
                        IncludeTemplate(config, child);
                        break;
                }
            }
        }

        private static (Dictionary<Uri, XmlMapping> Mappings, XmlMapping Mapping) CreateDefaultMappings()
        {
            var mapping = new XmlMapping();
            var mappings = new Dictionary<Uri, XmlMapping>
            {
                { new Uri("root:"), mapping },
            };

            return (mappings, mapping);
        }

        private string? GetTypeHandler(XElement element, string name)
        {
            var handler = GetAttribute(element, name);
            /*
            if (handler != null && !mapping.isName(Mapping.TYPE_HANDLER_NAMESPACE, handler)) {
                throw new BeanIOConfigurationException("Unresolved type handler named '" + handler + "'");
            }
            */
            return handler;
        }

        private string? DoPropertySubstitution(string? text)
        {
            try
            {
                return _propertySubstitutionEnabled ? StringUtil.DoPropertySubstitution(text, this) : text;
            }
            catch (ArgumentException ex)
            {
                throw new BeanIOConfigurationException(ex.Message, ex);
            }
        }

        private string? GetOptionalAttribute(XElement element, string name)
        {
            var attr = element.Attribute(name);
            if (attr == null)
                return null;
            return DoPropertySubstitution(attr.Value);
        }

        private bool HasAttribute(XElement element, string name)
        {
            return element.Attribute(name) != null;
        }

        private string? GetAttribute(XElement element, string name)
        {
            var attr = element.Attribute(name);
            string? value;
            if (attr == null)
                value = null;
            else if (string.IsNullOrEmpty(attr.Value))
                value = null;
            else
                value = attr.Value;
            return DoPropertySubstitution(value);
        }

        private int? GetIntAttribute(XElement element, string name)
        {
            var text = GetAttribute(element, name);
            if (string.IsNullOrEmpty(text))
                return null;
            return int.Parse(text);
        }

        private int? GetUnboundedIntAttribute(XElement element, string name, int? unboundedValue = null)
        {
            var text = GetAttribute(element, name);
            if (string.IsNullOrEmpty(text))
                return null;
            if (text == "unbounded")
                return unboundedValue;
            return int.Parse(text);
        }

        private char? GetCharacterAttribute(XElement element, string name)
        {
            var text = GetAttribute(element, name);
            if (string.IsNullOrEmpty(text))
                return null;
            return text![0];
        }

        private bool? GetBoolAttribute(XElement element, string name)
        {
            var text = GetAttribute(element, name);
            if (string.IsNullOrEmpty(text))
                return null;
            return XmlConvert.ToBoolean(text);
        }

        private T? GetEnumAttribute<T>(XElement element, string name)
            where T : struct
        {
            var text = GetAttribute(element, name);
            if (string.IsNullOrEmpty(text))
                return null;
            if (Enum.TryParse(text, true, out T result))
                return result;
            throw new BeanIOConfigurationException(string.Format("Invalid {1} '{0}'", text, name));
        }

        private T? GetOptionalEnumAttribute<T>(XElement element, string name)
            where T : struct
        {
            var text = GetOptionalAttribute(element, name);
            if (string.IsNullOrEmpty(text))
                return null;
            return (T)Enum.Parse(typeof(T), text, true);
        }

        private Range? GetRangeAttribute(XElement element, string name)
        {
            // parse occurs (e.g. '1', '0-1', '0+', '1+' etc)
            var range = GetAttribute(element, name);
            if (range == null)
                return null;

            try
            {
                int? min;
                int? max;

                if (range.EndsWith("+", StringComparison.Ordinal))
                {
                    min = int.Parse(range.Substring(0, range.Length - 1));
                    max = int.MaxValue;
                }
                else
                {
                    int n = range.IndexOf('-');
                    if (n == -1)
                    {
                        min = max = int.Parse(range);
                    }
                    else
                    {
                        min = int.Parse(range.Substring(0, n));
                        max = int.Parse(range.Substring(n + 1));
                    }
                }

                return new Range(min, max);
            }
            catch (FormatException ex)
            {
                throw new BeanIOConfigurationException($"Invalid {name} '{range}'", ex);
            }
        }

        private void PopulatePropertyConfig(PropertyConfig config, XElement element)
        {
            config.ValidateOnMarshal = GetBoolAttribute(element, "validateOnMarshal");
            config.Getter = GetAttribute(element, "getter");
            config.Setter = GetAttribute(element, "setter");
            config.Collection = GetAttribute(element, "collection");
            PopulatePropertyConfigOccurs(config, element);
        }

        private void PopulatePropertyConfigOccurs(PropertyConfig config, XElement element)
        {
            config.OccursRef = GetAttribute(element, "occursRef");

            var hasOccurs = HasAttribute(element, "occurs");
            if (hasOccurs)
            {
                if (HasAttribute(element, "minOccurs") || HasAttribute(element, "maxOccurs"))
                    throw new BeanIOConfigurationException("occurs cannot be used with minOccurs or maxOccurs");

                var range = GetRangeAttribute(element, "occurs");
                if (range != null)
                {
                    config.MinOccurs = range.Min;
                    config.MaxOccurs = range.Max;
                }
            }
            else
            {
                config.MinOccurs = GetIntAttribute(element, "minOccurs");
                config.MaxOccurs = GetUnboundedIntAttribute(element, "maxOccurs", int.MaxValue);
            }
        }

        private class Include
        {
            public Include(string template, int offset)
            {
                Template = template;
                Offset = offset;
            }

            public string Template { get; }

            public int Offset { get; }
        }

        private class Range
        {
            public Range(int? min, int? max)
            {
                Min = min;
                Max = max;
            }

            public int? Min { get; }

            public int? Max { get; }
        }
    }
}
